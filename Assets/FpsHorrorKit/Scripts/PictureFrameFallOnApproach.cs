using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public sealed class PictureFrameFallOnApproach : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Khoang cach kich hoat")]
    [SerializeField, Min(0.1f)]
    private float triggerDistance = 1f;

    [Header("Vat ly khi tranh roi")]
    [SerializeField, Min(0.1f)]
    private float mass = 3f;

    [SerializeField]
    private float outwardPush = 0.45f;

    [SerializeField]
    private float spinForce = 1.5f;

    [Header("Am thanh sau khi tranh cham dat")]
    [SerializeField] private AudioClip sound1;
    [SerializeField] private AudioClip sound2;

    [Header("Thoi gian cho")]
    [SerializeField] private float maxWaitForLanding = 5f;
    [SerializeField] private float delayBeforeSound1 = 0f;
    [SerializeField] private float delayBetweenSounds = 0f;

    private bool hasFallen;
    private bool hasLanded;

    private Rigidbody body;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }

        EnsureCollider();
    }

    private void Update()
    {
        if (hasFallen || player == null)
            return;

        Vector3 horizontalOffset =
            player.position - transform.position;

        horizontalOffset.y = 0f;

        if (horizontalOffset.sqrMagnitude <=
            triggerDistance * triggerDistance)
        {
            Drop();
        }
    }

    private void Drop()
    {
        if (hasFallen)
            return;

        hasFallen = true;

        // Tach tranh khoi vat the cha
        transform.SetParent(null, true);

        // Tao Rigidbody neu chua co
        body = GetComponent<Rigidbody>();

        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.mass = mass;
        body.isKinematic = false;
        body.useGravity = true;

        body.interpolation =
            RigidbodyInterpolation.Interpolate;

        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // Day tranh ra khoi tuong
        body.AddForce(
            -transform.forward * outwardPush,
            ForceMode.Impulse
        );

        // Tao hieu ung xoay khi roi
        body.AddTorque(
            transform.right * spinForce,
            ForceMode.Impulse
        );

        // Bat dau cho tranh cham dat
        StartCoroutine(WaitForLandingAndPlayAudio());
    }

    private IEnumerator WaitForLandingAndPlayAudio()
    {
        float elapsed = 0f;

        // Cho tranh cham dat, toi da 5 giay
        while (!hasLanded && elapsed < maxWaitForLanding)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Cho them truoc khi phat am thanh 1
        if (delayBeforeSound1 > 0f)
        {
            yield return new WaitForSeconds(delayBeforeSound1);
        }

        // Phat am thanh 1
        if (sound1 != null)
        {
            yield return StartCoroutine(
                PlaySoundAndWait(sound1)
            );
        }

        // Thoi gian nghi giua hai am thanh
        if (delayBetweenSounds > 0f)
        {
            yield return new WaitForSeconds(delayBetweenSounds);
        }

        // Phat am thanh 2
        if (sound2 != null)
        {
            yield return StartCoroutine(
                PlaySoundAndWait(sound2)
            );
        }
    }

    private IEnumerator PlaySoundAndWait(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            yield break;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = false;

        audioSource.Play();

        while (audioSource.isPlaying)
        {
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasFallen || hasLanded)
            return;

        // Chi tinh la cham dat neu mat va cham huong len
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                hasLanded = true;
                break;
            }
        }
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider>() != null)
            return;

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        bool hasBounds = false;
        Bounds localBounds = new Bounds();

        foreach (Renderer item in renderers)
        {
            Bounds worldBounds = item.bounds;

            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 corner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z
                        );

                        corner =
                            transform.InverseTransformPoint(corner);

                        if (!hasBounds)
                        {
                            localBounds =
                                new Bounds(corner, Vector3.zero);

                            hasBounds = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(corner);
                        }
                    }
                }
            }
        }

        if (hasBounds)
        {
            BoxCollider box =
                gameObject.AddComponent<BoxCollider>();

            box.center = localBounds.center;
            box.size = localBounds.size;

            // Dam bao collider du do day
            box.size = new Vector3(
                Mathf.Max(box.size.x, 0.05f),
                Mathf.Max(box.size.y, 0.05f),
                Mathf.Max(box.size.z, 0.05f)
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            triggerDistance
        );
    }
}