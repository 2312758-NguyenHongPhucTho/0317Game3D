# 03:17 – VÒNG LẶP TẾ HỒN

**Game kinh dị tâm lý góc nhìn thứ nhất (3D Psychological Horror) | Unity 6 | PC | Single-player**

> **03:17.** Bạn tỉnh dậy trong một căn biệt thự cổ ở Đà Lạt. Cánh cửa dẫn đến phòng bùa bị phong ấn, những đồ vật quen thuộc ẩn giấu bí mật, và mỗi lần tưởng như đã thoát, chiếc đồng hồ lại đưa bạn trở về một thời khắc không thể quên.

**03:17 – Vòng Lặp Tế Hồn** là dự án game 3D kinh dị tâm lý do **Nhóm 17** phát triển phục vụ học tập. Người chơi khám phá một căn biệt thự cổ, tìm manh mối, giải các phong ấn, thu thập và sử dụng chìa khóa, tránh bẫy, đồng thời khám phá sự thật về gia đình ông Lâm và nghi lễ diễn ra lúc **03:17**.

Trò chơi tập trung vào **khám phá môi trường, kể chuyện qua đồ vật, giải đố và không khí kinh dị tâm linh Việt Nam**, kết hợp những phân đoạn truy đuổi và cắt cảnh theo kịch bản.

> [!IMPORTANT]
> Dự án **đang trong quá trình phát triển (bản demo học phần)**. Các chức năng liệt kê bên dưới mô tả thiết kế và tiến độ đã triển khai; một số tương tác, hiệu ứng hình ảnh và cắt cảnh vẫn cần Play-test trực tiếp trong Unity trước khi coi là hoàn thiện.

---

## Mục lục

- [Thông tin dự án](#thông-tin-dự-án)
- [Cốt truyện](#cốt-truyện)
- [Gameplay và tính năng](#gameplay-và-tính-năng)
- [Hệ thống giải đố](#hệ-thống-giải-đố)
- [Bẫy và sinh tồn](#bẫy-và-sinh-tồn)
- [Điều khiển](#điều-khiển)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Cài đặt và chạy game](#cài-đặt-và-chạy-game)
- [Tiến độ phát triển](#tiến-độ-phát-triển)
- [Quy trình làm việc với Git](#quy-trình-làm-việc-với-git)
- [Tài nguyên và bản quyền](#tài-nguyên-và-bản-quyền)
- [Thành viên](#thành-viên)

---

## Thông tin dự án

| Hạng mục | Thông tin |
| --- | --- |
| **Tên game** | 03:17 – Vòng Lặp Tế Hồn |
| **Tên repository** | `0317Game3D` |
| **Thể loại** | Psychological Horror, Exploration, Puzzle, Survival |
| **Góc nhìn** | First-person (FPS) |
| **Chế độ** | Một người chơi (Single-player) |
| **Nền tảng mục tiêu** | PC / Windows |
| **Game engine** | Unity **6000.0.43f1** (Unity 6) |
| **Render pipeline** | High Definition Render Pipeline (HDRP) |
| **Ngôn ngữ giao diện** | Tiếng Việt có dấu (đang chuẩn hóa toàn bộ thông báo) |
| **Mục đích** | Dự án học tập / demo môn học |
| **Trạng thái** | Đang phát triển, chưa phát hành bản hoàn chỉnh |

**Repository:** <https://github.com/2312758-NguyenHongPhucTho/0317Game3D>

## Cốt truyện

Một người lữ khách lạc trong rừng Đà Lạt tìm đến căn biệt thự của **ông Lâm** để xin trú qua đêm. Khi tỉnh dậy, chiếc đồng hồ chỉ **03:17**. Ngôi nhà trở nên xa lạ: những căn phòng tối, các ghi chú bất thường, những tiếng động không rõ nguồn gốc và một cánh cửa bị bùa phong ấn.

Lần theo manh mối, người chơi dần phát hiện ông Lâm đã dùng tà thuật để thực hiện các nghi lễ và giam giữ linh hồn **vợ cùng con gái** trong hai con búp bê. Để tiến sâu hơn, người chơi phải giải phong ấn, tìm chìa khóa, khám phá phòng tế và đối mặt với hậu quả của việc phá vỡ nghi lễ.

<details>
<summary><strong>⚠️ Xem nội dung có tiết lộ cốt truyện (spoiler)</strong></summary>

- Sau khi giải thoát hai linh hồn bằng cách phá hủy những con búp bê nghi lễ, người chơi bị ông Lâm phát hiện tại phòng bùa.
- Ông Lâm khóa cửa, truy đuổi và tấn công người chơi. Cảnh sát hại được thể hiện bằng camera FPS, hiệu ứng sát thương, Blink và màn hình tối.
- Người chơi tỉnh dậy trên giường lúc **03:17** trong **Loop 2**, nhưng những dấu vết của vòng trước vẫn còn.
- Khi trở lại phòng bùa, người chơi nhìn thấy ông Lâm trước bàn thờ và **thi thể của chính mình** trong vòng tròn tế đàn.
- Cảnh kết thúc sử dụng chuyển động camera chậm quanh phòng, âm nhạc và thông điệp về vòng lặp.

*Nội dung, lời thoại và nhịp dựng ending còn có thể được điều chỉnh trong quá trình hoàn thiện demo.*

</details>

---

## Gameplay và tính năng

### Khám phá và tương tác

- Di chuyển tự do trong các khu vực của biệt thự, tìm vật phẩm và manh mối ẩn trong môi trường.
- Nhìn vào vật thể để **tương tác bằng phím `E`**; một số vật phẩm hỗ trợ chế độ **Inspect** để quan sát gần hơn.
- Đọc thư, ghi chú và ảnh để tìm hiểu cốt truyện cũng như điều kiện giải đố.
- Sử dụng **đèn pin** để quan sát trong bóng tối và phát hiện một số dấu vết đặc biệt.
- Các thông báo tương tác, phụ đề và hướng dẫn đang được thống nhất sang **tiếng Việt có dấu** qua TextMeshPro.

### Chìa khóa – không sử dụng Inventory dạng ô đồ

Game **không có màn hình Inventory truyền thống**. Thay vào đó, chìa khóa được quản lý bằng logic sở hữu và hiển thị trực tiếp trên camera FPS:

- Nhặt chìa khóa bằng tương tác trong thế giới game.
- Nhấn **`T`** để cầm/cất model chìa khóa ở **góc dưới bên trái** màn hình.
- Đèn pin vẫn hiển thị ở **bên phải** và dùng phím riêng, không bị ảnh hưởng khi cầm chìa khóa.
- Dùng **`E`** để tương tác cửa; cửa chỉ mở khi đáp ứng điều kiện khóa và phong ấn tương ứng.
- **Cơ chế tiêu thụ từng chìa khóa sau khi mở khóa thành công** là một hạng mục đang được tinh chỉnh và cần xác nhận Play-test với nhiều chìa khóa.

### Vật phẩm phục vụ cốt truyện

- **Hộp nhạc:** tương tác mở/đóng, phát/dừng âm thanh và cung cấp manh mối liên quan phong ấn.
- **Ảnh người mẹ:** hệ thống tương tác được thiết kế để nhặt, xem và sử dụng ảnh tại vị trí nghi lễ mà không cần Inventory; cần kiểm tra trạng thái tích hợp thực tế trong scene.
- **Búp bê nghi lễ:** giải bùa, kiểm tra và phá hủy khi đủ điều kiện.
- **Băng gạc:** nhặt để hồi máu.
- **Thư và ghi chú:** dẫn dắt người chơi khám phá câu chuyện và thứ tự giải đố.

### Cutscene và vòng lặp

- Phân đoạn ông Lâm xuất hiện sau khi cả hai búp bê bị phá hủy.
- Cửa phòng bùa bị khóa trong pha truy đuổi; ông Lâm tấn công theo kịch bản **ba đòn đánh**.
- Camera ngã xuống, hiệu ứng Blink và blackout để chuyển sang vòng chơi tiếp theo.
- **Loop 2** đưa người chơi trở lại giường mà không cần tải lại toàn bộ scene; hệ thống được thiết kế để giữ một số hậu quả của Loop 1.
- Khi người chơi quay lại phòng bùa, kích hoạt phân đoạn khám phá thi thể và **ending camera orbit**.
- **DOE Ghost** được sử dụng như model tạm cho ông Lâm; animation ngồi/tấn công và hình ảnh cuối còn cần hoàn thiện.

---

## Hệ thống giải đố

### Tam Ấn Phong Môn – phong ấn cửa phòng bùa

Để vào phòng bùa, người chơi phải tìm manh mối và kích hoạt ba dấu ấn theo đúng thứ tự:

**👁 Mắt → 🌙 Trăng khuyết → 🔥 Lửa**

| Dấu ấn | Vị trí thiết kế | Ý nghĩa gợi ý |
| --- | --- | --- |
| **Mắt** | Hành lang / gần gương hoặc khung tranh | Đôi mắt không bao giờ khép |
| **Trăng** | Khu vực phòng ngủ | Ánh trăng gắn với lời ru của người mẹ |
| **Lửa** | Hành lang gần cửa phòng bùa | Ngọn lửa cuối cùng của phong ấn |

Nếu chọn sai, tiến trình Tam Ấn được đặt lại và cơ chế lửa bảo vệ cửa có thể kích hoạt. Chọn đúng cả ba làm **suy yếu phong ấn**.

**Thiết kế mục tiêu:** Kích hoạt đủ ba dấu ấn **chưa tự phá lá bùa**. Người chơi phải **quay về cửa, tự gỡ bùa**, sau đó dùng **đúng chìa khóa** để mở cửa. Phần tách trạng thái “đã giải ba ấn” và “đã tự gỡ bùa” đang trong quá trình tích hợp/kiểm thử; không nên mặc định đã hoàn thiện nếu chưa kiểm tra bản scene mới nhất.

### Các điều kiện giải bùa khác

Các búp bê nghi lễ có điều kiện giải bùa riêng, liên quan đến manh mối như hộp nhạc và ảnh người mẹ. Cơ chế này được quản lý độc lập với bùa niêm phong cửa phòng bùa.

---

## Bẫy và sinh tồn

Người chơi có **100 HP** và có thể nhận sát thương từ bẫy trong biệt thự. Thanh máu đặt ở **góc trên bên trái** màn hình.

| Bẫy / cơ chế | Sát thương thiết kế | Hành vi |
| --- | --- | --- |
| **Bẫy gấu (BearTrap)** | **30 HP/lần** | Giữ chân người chơi, camera nhìn xuống; nhấn **`E` 8 lần** để thoát. Model hiện dùng dạng tĩnh. |
| **Kính vỡ** | **6 HP/giây** | Gây sát thương liên tục khi Player ở trong trigger; đã chỉnh collider, vẫn cần xác nhận bằng di chuyển FPS thực tế. |
| **Lửa phong ấn** | **10 HP/giây** | Chỉ bùng lên trong điều kiện phong ấn/cửa liên quan, tồn tại tối đa khoảng 3 giây mỗi lần kích hoạt. |
| **Băng gạc** | **+30 HP** | Hồi máu, tối đa không vượt quá HP tối đa. |

### Phản hồi sát thương

- Sau khi nhận sát thương, **viền màn hình đỏ trong khoảng 5 giây**, rồi trở lại bình thường nếu còn trên 30% HP.
- Khi HP **≤ 30%**, viền đỏ duy trì cho tới khi hồi lên trên ngưỡng đó.
- Trung tâm màn hình trong suốt để người chơi vẫn quan sát được môi trường.
- Khi HP về 0, hiệu ứng nhường màn hình cho phân đoạn chết/Blink; hồi sinh phục hồi trạng thái theo thiết kế.
- Hệ thống chết do bẫy và cái chết theo kịch bản ông Lâm được xử lý riêng để tránh làm gián đoạn Loop 2.

---

## Điều khiển

| Phím | Chức năng |
| --- | --- |
| `W` `A` `S` `D` | Di chuyển |
| `Chuột` | Nhìn xung quanh |
| `Shift` | Chạy (Sprint) |
| `E` | Tương tác, nhặt vật phẩm, thao tác cửa / dấu ấn |
| `T` | Hiện / cất chìa khóa đang sở hữu |
| `R` | Trang bị / cất đèn pin theo hệ thống đèn pin hiện tại |
| `X` | Phá vật phẩm phù hợp trong chế độ Inspect (khi thỏa điều kiện) |
| `F` | Xem / cất ảnh mẹ **nếu** hệ thống ảnh đã được gắn và không xung đột input |
| `Esc` | Menu / tạm dừng theo cấu hình của scene |

**Tình huống đặc biệt:** Khi mắc bẫy gấu, nhấn `E` liên tục đủ 8 lần để gỡ. Tương tác gỡ bùa cửa có thể dùng giữ `E` theo phiên bản gameplay đang được hoàn thiện.

> Các phím trong bảng dựa trên thiết kế và các script đã triển khai. Hãy kiểm tra lại **Game View** nếu scene hoặc Input Actions được chỉnh sửa.

---

## Công nghệ sử dụng

| Công nghệ / công cụ | Vai trò |
| --- | --- |
| **Unity 6 (6000.0.43f1)** | Game engine và Unity Editor |
| **C#** | Gameplay và hệ thống tương tác |
| **HDRP** | Render, ánh sáng và hiệu ứng hình ảnh |
| **Unity Input System** | Input FPS và tương tác |
| **Unity Physics / CharacterController** | Chuyển động, trigger và va chạm |
| **Unity UI / TextMeshPro** | HUD, phụ đề, hướng dẫn tương tác |
| **FpsHorrorKit** | Nền tảng controller và các hệ thống FPS đang sử dụng |
| **Git / GitHub** | Quản lý phiên bản và phối hợp nhóm |
| **Unity MCP + Codex** | Hỗ trợ kiểm tra, cấu hình scene và phát triển trong Editor |

Một số tài nguyên bên ngoài có thể cần package tương ứng (ví dụ glTFast cho GLB/glTF) hoặc phần mềm chuyển đổi. Hãy kiểm tra `Packages/manifest.json` của phiên bản repository được clone trước khi cài thêm package.

---

## Cấu trúc dự án

Các đường dẫn dưới đây nêu những khu vực chính **được sử dụng trong quá trình phát triển**; không phải danh sách đầy đủ mọi asset hoặc script:

```text
0317Game3D/
├── Assets/
│   ├── FpsHorrorKit/
│   │   ├── Scenes/
│   │   │   └── Tho.unity                 # Scene gameplay chính
│   │   ├── Scripts/
│   │   │   ├── Systems/                 # Bẫy, máu, cửa, nghi lễ, tương tác...
│   │   │   └── PlayerBlinkEffect.cs
│   │   └── Animations/
│   ├── CacLoaiBay/                      # Asset và model bẫy nhập vào dự án
│   └── ...                              # Các model, vật liệu, prefab, audio, UI khác
├── Packages/
│   └── manifest.json
├── ProjectSettings/
├── .gitignore
├── README.md
└── ...
```

Ví dụ một số script / hệ thống đang được phát triển:

- `PlayerHealth0317`, `HealthBar0317`, `DamageVignette0317`
- `DamageTrap0317`, `TrapEscapeZone0317`, `TrapEscapeHandler0317`, `TrapGroup0317`
- `DoorSystem`, `RitualRoomDoorLock`, `RitualDoorSeal0317`, `RitualSealSymbol0317`
- `RitualManager`, `InspectSystem`, `MusicBoxInteract`
- `LamAttackCutscene`, `Loop2ReturnTrigger`, `PlayerBlinkEffect`
- `DoorKeyPickup`, `DoorKeyUseController` (hệ thống chìa khóa hiện có)

Đường dẫn các script có thể khác nhau giữa các thư mục asset của project. Sử dụng **Unity Project window** hoặc tìm tên class để xác định đường dẫn thực tế.

---

## Cài đặt và chạy game

### Yêu cầu

- **Windows PC** và **Unity Hub**.
- **Unity Editor 6000.0.43f1**, ưu tiên đúng phiên bản trong `ProjectSettings/ProjectVersion.txt`.
- Git (khuyến nghị) và Git LFS nếu repository sử dụng LFS cho model/audio dung lượng lớn.
- Đủ dung lượng ổ đĩa cho việc import asset và tạo thư mục `Library/`.

### 1. Clone repository

```bash
git clone https://github.com/2312758-NguyenHongPhucTho/0317Game3D.git
cd 0317Game3D
```

> Không dùng URL có `/tree/main` với `git clone`; đó là đường dẫn trang web GitHub, không phải địa chỉ clone repository.

Nếu dự án đã cấu hình Git LFS:

```bash
git lfs install
git lfs pull
```

### 2. Mở bằng Unity Hub

1. Mở **Unity Hub → Add project from disk** (hoặc **Add**).
2. Chọn **thư mục gốc `0317Game3D`**, nơi chứa `Assets/`, `Packages/` và `ProjectSettings/`.
3. Mở bằng **Unity 6000.0.43f1**.
4. Chờ Unity import asset, khôi phục package và compile C# hoàn tất.
5. Mở scene `Assets/FpsHorrorKit/Scenes/Tho.unity`.
6. Kiểm tra Console và nhấn **Play** để thử trong Editor.

### 3. Lưu ý khi Play-test

- Để tránh giữ trạng thái runtime của lần Play trước, có thể **tạm tắt** `Edit → Project Settings → Editor → Enter Play Mode Options` trong lúc kiểm thử chức năng cần khởi tạo lại scene.
- Các tính năng liên quan `TrapEscapeHandler0317`, HUD và các đối tượng được tạo ở runtime nên được kiểm tra sau khi **thoát Play Mode và vào lại**.
- Nếu model `.blend` không import, hãy dùng file FBX/OBJ hoặc chuyển sang FBX bằng Blender. Unity không thể sử dụng trực tiếp một số file `.blend` nếu máy thiếu Blender.
- **Chưa có file build Windows được xác nhận phát hành** trong README này. Có thể chạy demo qua Unity Editor; cấu hình build theo yêu cầu của nhóm.

---

## Tiến độ phát triển

| Hạng mục | Tình trạng |
| --- | --- |
| Bộ điều khiển FPS và hệ thống tương tác | ✅ Đã triển khai |
| Bối cảnh biệt thự, khám phá, manh mối và các vật phẩm | ✅ Đã xây dựng nền tảng; tiếp tục hoàn thiện |
| Đèn pin và hộp nhạc tương tác | ✅ Đã triển khai; cần kiểm tra asset/âm thanh ở scene hiện tại |
| Thanh HP, băng gạc và phản hồi sát thương | ✅ Logic đã được kiểm tra |
| Bẫy gấu | ✅ Gameplay đã có và đã thử các luồng chính |
| Kính vỡ / lửa phong ấn | 🧪 Đã chỉnh cấu hình; cần kiểm chứng va chạm FPS thực tế |
| Tam Ấn Phong Môn | 🧪 Đã có logic thứ tự; đang hoàn thiện tương tác tự gỡ bùa |
| Chìa khóa cầm tay trái / đèn pin bên phải | 🧪 Đã cấu hình; cần Play-test đồng thời và việc tiêu thụ chìa khóa |
| Giải bùa và phá hai búp bê | 🧪 Đã xây dựng luồng gameplay; cần kiểm tra toàn tuyến |
| Ông Lâm truy đuổi → Loop 2 → Ending | 🧪 Đã cấu hình cutscene và các điểm nối; chưa xác nhận toàn bộ bằng một lượt chơi liên tục |
| Phụ đề và thông báo tiếng Việt có dấu | 🔧 Đang rà soát/đồng bộ font và text |
| Model, animation, âm thanh, hiệu ứng hoàn chỉnh | 🔧 Đang bổ sung và tối ưu |
| Bản build và kiểm thử phát hành | ⏳ Chưa xác nhận hoàn tất |

**Quy ước:** ✅ Đã triển khai / xác nhận phần chính · 🧪 Cần Play-test thêm · 🔧 Đang hoàn thiện · ⏳ Chưa hoàn tất.

---

## Quy trình làm việc với Git

### Cập nhật trước khi làm việc

```bash
git status
git pull --rebase origin main
```

Nếu có thay đổi chưa commit, hãy **commit hoặc lưu tạm bằng `git stash`** trước khi `pull --rebase`; không ghi đè scene đang làm dở.

### Commit và push

**Trong Unity:** Thoát Play Mode, kiểm tra scene và nhấn **Ctrl + S** trước khi commit để giữ những thay đổi trong `Tho.unity`.

```bash
git status
git add -A
git commit -m "Update 03:17 gameplay, puzzles, traps and documentation"
git pull --rebase origin main
git push origin main
git status
```

**Chỉ cập nhật README:**

```bash
git add README.md
git commit -m "docs: update 03:17 README"
git push origin main
```

Không chạy `git push --force` để xử lý xung đột trên nhánh chung. Nếu `pull --rebase` báo conflict, giải quyết xung đột rồi mới push.

### Những thư mục nên bỏ qua trong `.gitignore`

```gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Ll]ogs/
[Bb]uild/
[Bb]uilds/
UserSettings/
```

Cần **giữ các file `.meta`** đi kèm asset Unity khi commit. Không commit file backup tạm, thông tin nhạy cảm hoặc dữ liệu Editor không cần thiết. Với file lớn, kiểm tra giới hạn GitHub và cấu hình **Git LFS** trước khi đưa vào commit.

---

## Tài nguyên và bản quyền

- Game dùng các asset tự tạo và asset bên thứ ba (model, texture, animation, âm thanh, hiệu ứng) theo điều kiện giấy phép tương ứng.
- **Không phải mọi asset “miễn phí tải” đều là CC0.** Một số asset Sketchfab có giấy phép **CC BY**, yêu cầu ghi công tác giả và liên kết tới nguồn; asset từ Unity Asset Store/Fab có điều khoản riêng.
- Nên lập danh sách ghi công asset (`ASSET_CREDITS.md` hoặc một mục trong báo cáo nhóm), bao gồm **tên asset, tác giả, URL, giấy phép và thay đổi nếu có**.
- Mã nguồn, model, âm thanh và các thành phần bên thứ ba **không tự động được cấp phép lại** chỉ vì repository phục vụ học tập.
- Khi phát hành hoặc chia sẻ bản build, cần kiểm tra quyền sử dụng của tất cả tài nguyên đi kèm.

**Mục đích của dự án:** học tập, thực hành Unity/C#, xây dựng môi trường 3D, giải đố, thiết kế game và cộng tác Git/GitHub.

© 2026 **Nhóm 17 — 03:17 – Vòng Lặp Tế Hồn**.

---

## Thành viên

**Nhóm 17**

- **Lương Hữu Thiện**
- **Nguyễn Hồng Phúc Thọ**
- **Hoàng Bình Quân**
- **Nguyễn Minh Long**

---

*“03:17 — Có những vòng lặp không bắt đầu từ chiếc đồng hồ.”*
