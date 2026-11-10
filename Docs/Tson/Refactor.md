# Refactor tài nguyên — Nhật ký thay đổi chưa commit

> Tài liệu này ghi lại **toàn bộ việc di chuyển / đổi tên tài nguyên dưới `Assets/`** trong lần refactor hiện tại.
> Mục đích: khi có lỗi phát sinh (mất tham chiếu, Missing script/material, build lỗi…) thì còn dấu vết để tìm lại hoặc khôi phục.

| Mục | Giá trị |
| --- | --- |
| Nhánh hiện tại | `refactor/change_asset` |
| Commit gần nhất | `009edb1` — "temp refactor 2" |
| Trạng thái | **Chưa commit** — toàn bộ nằm trong working tree |
| Người thực hiện | Helper hỗ trợ project (không phải thành viên chính thức) |
| Ngày ghi nhận | 08/11/2026 |
| Phạm vi | Chỉ các thay đổi dưới `Assets/`. Các thay đổi ngoài `Assets/` được liệt kê sơ lược ở mục 10. |

## 0. Cách đọc tài liệu

- **Mục 1** — số liệu tổng quan.
- **Mục 2** — bảng ánh xạ **thư mục** (nên đọc trước): thư mục cũ đã đi đâu.
- **Mục 3** — danh sách **file được đổi tên thật sự** (khác tên file, không chỉ khác thư mục).
- **Mục 4** — các file **đổi tên + có sửa nội dung**.
- **Mục 5** — **danh sách đầy đủ 1.212 file** đã di chuyển, nhóm theo cặp thư mục cũ → mới.
- **Mục 6–9** — thư mục mới, file bị xoá, các trường hợp đặc biệt và cảnh báo.
- **Mục 10–12** — khôi phục, ngoài phạm vi, quyết định đã chốt & câu hỏi còn lại.

> **Quy ước:** mỗi file asset đều đi kèm một file `.meta` cùng tên. Trong các bảng bên dưới chỉ ghi file asset;
> file `.meta` tương ứng được di chuyển/đổi tên y hệt (thêm hậu tố `.meta`).

> **Về GUID:** Unity tham chiếu tài nguyên bằng `guid` trong file `.meta`, không bằng đường dẫn.
> Đã kiểm tra: **toàn bộ `.meta` của 1.212 file được di chuyển vẫn giữ nguyên GUID** → tham chiếu trong scene/prefab/material không bị ảnh hưởng.
> Ngoại lệ duy nhất là 7 `.meta` của **thư mục** bị Unity cấp GUID mới (xem mục 9.1) — không tìm thấy tham chiếu nào tới các GUID cũ này.
> Ngoài ra, 6 thư mục đích rỗng đã bị xoá cùng `.meta` theo yêu cầu (mục 9.2 / 8.3) — cũng không có tham chiếu nào tới GUID của chúng.

### 0.1. Phương pháp ghi nhận

- Đối tượng: toàn bộ thay đổi **chưa commit** tại thời điểm ghi (`git status` + so sánh với `HEAD` = `009edb1`).
- Nhận diện rename: `git diff -M --find-renames=40%` trên một **index tạm** (không thay đổi index thật của repo).
- Kiểm chứng lại bằng **GUID trong file `.meta`**: nếu `.meta` cũ và mới có cùng `guid` thì chắc chắn là cùng một asset (chỉ đổi vị trí/tên).
- Các file **không tìm thấy** trong tài liệu này có thể là: file mới hoàn toàn, file nằm ngoài `Assets/`, hoặc file không được git nhận diện là rename do nội dung khác biệt quá nhiều.

## 1. Tổng quan số liệu

| Loại thay đổi | Số lượng | Ghi chú |
| --- | ---: | --- |
| File asset được di chuyển (giữ nguyên tên) | 1192 | Chỉ đổi thư mục |
| File asset được đổi tên | 20 | Đổi tên file; 2 file trong đó có sửa nội dung |
| &nbsp;&nbsp;↳ **Tổng file asset thay đổi vị trí/tên** | **1212** | Kèm 1212 file `.meta` (GUID giữ nguyên) |
| Thư mục được di chuyển | 165 | 158 giữ GUID, 7 bị cấp GUID mới; **6 thư mục đích rỗng đã xoá** (mục 9.2) |
| `.meta` thư mục mới (thư mục mới tạo) | 15 | Do Unity tự sinh |
| `.meta` thư mục bị xoá (không có thay thế) | 12 | 6 thư mục cũ + 6 thư mục đích rỗng đã xoá (mục 9.2) |
| File asset bị xoá | 0 | Không có asset nào bị xoá |
| File `.meta` bị sửa nội dung | 2 | Chỉnh import settings của model |
| Material bị Unity serialize lại (không đổi tên) | 10 | Thuộc pack Vefects — ngoài phạm vi refactor |

Phân bố theo đích đến:

| Thư mục gốc mới | Số file asset | Nguồn cũ |
| --- | ---: | --- |
| `Assets/_ThirdParty/` | 913 | FpsHorrorKit, Sky, Tree, Free-FlashLight |
| `Assets/_Project/` | 265 | _0317, Audio, Image, Materials, Prefabs, Player, Ghost, Mame, MACon, MaConBay, MAMe 1, Nen, OngLam, Quantai, Rain, RoomAudio, Studio, Tuong, AnhFull, BoXuong, DiaryImages, HopSo, Dau_ThienLinh |
| `Assets/_Unknown/` | 34 | CacLoaiBay, BuddhistFuneralAltar, bupbe |

## 2. Bảng ánh xạ thư mục (thay đổi vị trí)

Đây là bản đồ nhanh nhất: tìm thư mục cũ ở cột trái, xem thư mục mới ở cột phải.
Các trường hợp một thư mục cũ bị **tách** sang nhiều thư mục mới sẽ xuất hiện thành nhiều dòng.

| # | Thư mục cũ | Thư mục mới | Ghi chú |
| ---: | --- | --- | --- |
| 1 | `Assets/_0317/Prefabs` | `Assets/_Project/Prefabs` | GUID giữ nguyên |
| 2 | `Assets/_0317/Prefabs/Environment` | `Assets/_Project/Prefabs/Environment` | GUID giữ nguyên |
| 3 | `Assets/_0317/Prefabs/Gameplay` | `Assets/_Project/Prefabs/Gameplay` | GUID giữ nguyên |
| 4 | `Assets/_0317/Scripts` | `Assets/_Project/Scripts` | GUID giữ nguyên |
| 5 | `Assets/_0317/Scripts/Gameplay` | `Assets/_Project/Scripts/Gameplay` | GUID giữ nguyên |
| 6 | `Assets/AnhFull` | `Assets/_Project/Materials/AnhFull` | ⚠️ `.meta` bị cấp GUID mới |
| 7 | `Assets/AnhFull/Materials` | `Assets/_Project` | ⚠️ `.meta` bị cấp GUID mới |
| 8 | `Assets/Audio` | `Assets/_Project/Audio` | GUID giữ nguyên |
| 9 | `Assets/BoXuong` | `Assets/_Project/Image/OngLam` | ⚠️ `.meta` bị cấp GUID mới |
| 10 | `Assets/BuddhistFuneralAltar` | `Assets/_Unknown/BuddhistFuneralAltar` | GUID giữ nguyên |
| 11 | `Assets/BuddhistFuneralAltar/textures` | `Assets/_Unknown/BuddhistFuneralAltar/textures` | GUID giữ nguyên |
| 12 | `Assets/bupbe` | `Assets/_Unknown/bupbe` | GUID giữ nguyên |
| 13 | `Assets/CacLoaiBay` | `Assets/_Unknown/CacLoaiBay` | GUID giữ nguyên |
| 14 | `Assets/CacLoaiBay/3DauAn` | `Assets/_Unknown/CacLoaiBay/3DauAn` | GUID giữ nguyên |
| 15 | `Assets/CacLoaiBay/3DauAn/eye` | `Assets/_Unknown/CacLoaiBay/3DauAn/eye` | GUID giữ nguyên |
| 16 | `Assets/CacLoaiBay/3DauAn/eye/Materials` | `Assets/_Unknown/CacLoaiBay/3DauAn/eye/Materials` | GUID giữ nguyên |
| 17 | `Assets/CacLoaiBay/3DauAn/Fire` | `Assets/_Unknown/CacLoaiBay/3DauAn/Fire` | GUID giữ nguyên |
| 18 | `Assets/CacLoaiBay/3DauAn/Moon` | `Assets/_Unknown/CacLoaiBay/3DauAn/Moon` | GUID giữ nguyên |
| 19 | `Assets/CacLoaiBay/Bandage` | `Assets/_Unknown/CacLoaiBay/Bandage` | GUID giữ nguyên |
| 20 | `Assets/CacLoaiBay/Bandage/Materials` | `Assets/_Unknown/CacLoaiBay/Bandage/Materials` | GUID giữ nguyên |
| 21 | `Assets/CacLoaiBay/Bandage/textures` | `Assets/_Unknown/CacLoaiBay/Bandage/textures` | GUID giữ nguyên |
| 22 | `Assets/CacLoaiBay/BayGau` | `Assets/_Unknown/CacLoaiBay/BayGau` | GUID giữ nguyên |
| 23 | `Assets/CacLoaiBay/BayGau/beartrap` | `Assets/_Unknown/CacLoaiBay/BayGau/beartrap` | GUID giữ nguyên |
| 24 | `Assets/Dau_ThienLinh` | `Assets/_Project/Image/Dau_ThienLinh` | ⚠️ `.meta` bị cấp GUID mới |
| 25 | `Assets/Dau_ThienLinh/Materials` | `Assets/_Project/Image/Texture/Blood` | ⚠️ `.meta` bị cấp GUID mới |
| 26 | `Assets/DiaryImages` | `Assets/_Project/Image/DiaryImages` | ⚠️ `.meta` bị cấp GUID mới |
| 27 | `Assets/DiaryImages/Materials` | `Assets/_Project/Image/DiaryImages/Materials` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 28 | `Assets/DiaryImages/New Folder` | `Assets/_Project/Image/DiaryImages/New Folder` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 29 | `Assets/FpsHorrorKit` | `Assets/_ThirdParty/FpsHorrorKit` | GUID giữ nguyên |
| 30 | `Assets/FpsHorrorKit/Animations` | `Assets/_ThirdParty/FpsHorrorKit/Animations` | GUID giữ nguyên |
| 31 | `Assets/FpsHorrorKit/Audio` | `Assets/_ThirdParty/FpsHorrorKit/Audio` | GUID giữ nguyên |
| 32 | `Assets/FpsHorrorKit/CameraSettings` | `Assets/_ThirdParty/FpsHorrorKit/CameraSettings` | GUID giữ nguyên |
| 33 | `Assets/FpsHorrorKit/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Materials` | GUID giữ nguyên |
| 34 | `Assets/FpsHorrorKit/Materials/DecalMaterials` | `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials` | GUID giữ nguyên |
| 35 | `Assets/FpsHorrorKit/Materials/DecalMaterials/TextDecals` | `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials/TextDecals` | GUID giữ nguyên |
| 36 | `Assets/FpsHorrorKit/Materials/ExorcismCircle` | `Assets/_ThirdParty/FpsHorrorKit/Materials/ExorcismCircle` | GUID giữ nguyên |
| 37 | `Assets/FpsHorrorKit/Materials/Interactive` | `Assets/_ThirdParty/FpsHorrorKit/Materials/Interactive` | GUID giữ nguyên |
| 38 | `Assets/FpsHorrorKit/Models` | `Assets/_ThirdParty/FpsHorrorKit/Models` | GUID giữ nguyên |
| 39 | `Assets/FpsHorrorKit/Models/Carpet` | `Assets/_ThirdParty/FpsHorrorKit/Models/Carpet` | GUID giữ nguyên |
| 40 | `Assets/FpsHorrorKit/Models/CellarWalls` | `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls` | GUID giữ nguyên |
| 41 | `Assets/FpsHorrorKit/Models/CellarWalls/MatAndTex` | `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls/MatAndTex` | GUID giữ nguyên |
| 42 | `Assets/FpsHorrorKit/Models/Doors` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors` | GUID giữ nguyên |
| 43 | `Assets/FpsHorrorKit/Models/Doors/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Materials` | GUID giữ nguyên |
| 44 | `Assets/FpsHorrorKit/Models/Doors/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Textures` | GUID giữ nguyên |
| 45 | `Assets/FpsHorrorKit/Models/Floors` | `Assets/_ThirdParty/FpsHorrorKit/Models/Floors` | GUID giữ nguyên |
| 46 | `Assets/FpsHorrorKit/Models/Furnitures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures` | GUID giữ nguyên |
| 47 | `Assets/FpsHorrorKit/Models/Furnitures/Lights` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Lights` | GUID giữ nguyên |
| 48 | `Assets/FpsHorrorKit/Models/Furnitures/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Materials` | GUID giữ nguyên |
| 49 | `Assets/FpsHorrorKit/Models/Furnitures/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Textures` | GUID giữ nguyên |
| 50 | `Assets/FpsHorrorKit/Models/ModulerParts` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts` | GUID giữ nguyên |
| 51 | `Assets/FpsHorrorKit/Models/ModulerParts/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Materials` | GUID giữ nguyên |
| 52 | `Assets/FpsHorrorKit/Models/ModulerParts/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Textures` | GUID giữ nguyên |
| 53 | `Assets/FpsHorrorKit/Models/NewModels` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels` | GUID giữ nguyên |
| 54 | `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` | GUID giữ nguyên |
| 55 | `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` | GUID giữ nguyên |
| 56 | `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` | GUID giữ nguyên |
| 57 | `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` | GUID giữ nguyên |
| 58 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` | GUID giữ nguyên |
| 59 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` | GUID giữ nguyên |
| 60 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` | GUID giữ nguyên |
| 61 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` | GUID giữ nguyên |
| 62 | `Assets/FpsHorrorKit/Models/NewModels/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/textures` | GUID giữ nguyên |
| 63 | `Assets/FpsHorrorKit/Models/NewModels/UtilityBox` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox` | GUID giữ nguyên |
| 64 | `Assets/FpsHorrorKit/Models/NewModels/UtilityBox/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox/textures` | GUID giữ nguyên |
| 65 | `Assets/FpsHorrorKit/Models/Props` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props` | GUID giữ nguyên |
| 66 | `Assets/FpsHorrorKit/Models/Props/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Materials` | GUID giữ nguyên |
| 67 | `Assets/FpsHorrorKit/Models/Props/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Textures` | GUID giữ nguyên |
| 68 | `Assets/FpsHorrorKit/Models/Stairs` | `Assets/_ThirdParty/FpsHorrorKit/Models/Stairs` | GUID giữ nguyên |
| 69 | `Assets/FpsHorrorKit/Png` | `Assets/_ThirdParty/FpsHorrorKit/Png` | GUID giữ nguyên |
| 70 | `Assets/FpsHorrorKit/Png/Blood` | `Assets/_ThirdParty/FpsHorrorKit/Png/Blood` | GUID giữ nguyên |
| 71 | `Assets/FpsHorrorKit/Png/Blood/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Png/Blood/Materials` | GUID giữ nguyên |
| 72 | `Assets/FpsHorrorKit/Png/Item` | `Assets/_ThirdParty/FpsHorrorKit/Png/Item` | GUID giữ nguyên |
| 73 | `Assets/FpsHorrorKit/Png/Papers` | `Assets/_ThirdParty/FpsHorrorKit/Png/Papers` | GUID giữ nguyên |
| 74 | `Assets/FpsHorrorKit/Png/ScriptIcons` | `Assets/_ThirdParty/FpsHorrorKit/Png/ScriptIcons` | GUID giữ nguyên |
| 75 | `Assets/FpsHorrorKit/Png/UI` | `Assets/_ThirdParty/FpsHorrorKit/Png/UI` | GUID giữ nguyên |
| 76 | `Assets/FpsHorrorKit/Prefabs` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs` | GUID giữ nguyên |
| 77 | `Assets/FpsHorrorKit/Prefabs/Cellar` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Cellar` | GUID giữ nguyên |
| 78 | `Assets/FpsHorrorKit/Prefabs/Decals` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Decals` | GUID giữ nguyên |
| 79 | `Assets/FpsHorrorKit/Prefabs/Doors` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Doors` | GUID giữ nguyên |
| 80 | `Assets/FpsHorrorKit/Prefabs/Floors` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Floors` | GUID giữ nguyên |
| 81 | `Assets/FpsHorrorKit/Prefabs/Furnitures` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures` | GUID giữ nguyên |
| 82 | `Assets/FpsHorrorKit/Prefabs/Furnitures/Lights` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures/Lights` | GUID giữ nguyên |
| 83 | `Assets/FpsHorrorKit/Prefabs/InteractablePrefabs` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/InteractablePrefabs` | GUID giữ nguyên |
| 84 | `Assets/FpsHorrorKit/Prefabs/ModularParts` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModularParts` | GUID giữ nguyên |
| 85 | `Assets/FpsHorrorKit/Prefabs/ModulerWalls` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModulerWalls` | GUID giữ nguyên |
| 86 | `Assets/FpsHorrorKit/Prefabs/Player` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Player` | GUID giữ nguyên |
| 87 | `Assets/FpsHorrorKit/Prefabs/Props` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Props` | GUID giữ nguyên |
| 88 | `Assets/FpsHorrorKit/Prefabs/Systems` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Systems` | GUID giữ nguyên |
| 89 | `Assets/FpsHorrorKit/Scenes` | `Assets/_ThirdParty/FpsHorrorKit/Scenes` | GUID giữ nguyên |
| 90 | `Assets/FpsHorrorKit/Scenes/InventorySystemScene` | `Assets/_ThirdParty/FpsHorrorKit/Scenes/InventorySystemScene` | GUID giữ nguyên |
| 91 | `Assets/FpsHorrorKit/Scenes/MainScene` | `Assets/_ThirdParty/FpsHorrorKit/Scenes/MainScene` | GUID giữ nguyên |
| 92 | `Assets/FpsHorrorKit/Scripts` | `Assets/_ThirdParty/FpsHorrorKit/Scripts` | GUID giữ nguyên |
| 93 | `Assets/FpsHorrorKit/Scripts/DialogueSystems` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems` | GUID giữ nguyên |
| 94 | `Assets/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` | GUID giữ nguyên |
| 95 | `Assets/FpsHorrorKit/Scripts/Editor` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/Editor` | GUID giữ nguyên |
| 96 | `Assets/FpsHorrorKit/Scripts/FpsController` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/FpsController` | GUID giữ nguyên |
| 97 | `Assets/FpsHorrorKit/Scripts/InteractSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem` | GUID giữ nguyên |
| 98 | `Assets/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` | GUID giữ nguyên |
| 99 | `Assets/FpsHorrorKit/Scripts/InventorySyste` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste` | GUID giữ nguyên |
| 100 | `Assets/FpsHorrorKit/Scripts/InventorySyste/Items` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste/Items` | GUID giữ nguyên |
| 101 | `Assets/FpsHorrorKit/Scripts/PhotoCaptureSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/PhotoCaptureSystem` | GUID giữ nguyên |
| 102 | `Assets/FpsHorrorKit/Scripts/Systems` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/Systems` | GUID giữ nguyên |
| 103 | `Assets/FpsHorrorKit/Scripts/TrapSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/TrapSystem` | GUID giữ nguyên |
| 104 | `Assets/FpsHorrorKit/Shaders` | `Assets/_ThirdParty/FpsHorrorKit/Shaders` | GUID giữ nguyên |
| 105 | `Assets/FpsHorrorKit/TextMesh Pro` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro` | GUID giữ nguyên |
| 106 | `Assets/FpsHorrorKit/TextMesh Pro/Fonts` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Fonts` | GUID giữ nguyên |
| 107 | `Assets/FpsHorrorKit/TextMesh Pro/Resources` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources` | GUID giữ nguyên |
| 108 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` | GUID giữ nguyên |
| 109 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` | GUID giữ nguyên |
| 110 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` | GUID giữ nguyên |
| 111 | `Assets/FpsHorrorKit/TextMesh Pro/Shaders` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Shaders` | GUID giữ nguyên |
| 112 | `Assets/FpsHorrorKit/TextMesh Pro/Sprites` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Sprites` | GUID giữ nguyên |
| 113 | `Assets/FpsHorrorKit/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Textures` | GUID giữ nguyên |
| 114 | `Assets/FpsHorrorKit/Textures/Decal` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal` | GUID giữ nguyên |
| 115 | `Assets/FpsHorrorKit/Textures/Decal/Texts` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal/Texts` | GUID giữ nguyên |
| 116 | `Assets/FpsHorrorKit/Textures/Patterns` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns` | GUID giữ nguyên |
| 117 | `Assets/FpsHorrorKit/Textures/Patterns/Colorful` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/Colorful` | GUID giữ nguyên |
| 118 | `Assets/FpsHorrorKit/Textures/Patterns/FavoritePatterns` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/FavoritePatterns` | GUID giữ nguyên |
| 119 | `Assets/Free-FlashLight` | `Assets/_ThirdParty/Free-FlashLight` | GUID giữ nguyên |
| 120 | `Assets/Free-FlashLight/Materials` | `Assets/_ThirdParty/Free-FlashLight/Materials` | GUID giữ nguyên |
| 121 | `Assets/Free-FlashLight/Models` | `Assets/_ThirdParty/Free-FlashLight/Models` | GUID giữ nguyên |
| 122 | `Assets/Free-FlashLight/Prefabs` | `Assets/_ThirdParty/Free-FlashLight/Prefabs` | GUID giữ nguyên |
| 123 | `Assets/Free-FlashLight/Scenes` | `Assets/_ThirdParty/Free-FlashLight/Scenes` | GUID giữ nguyên |
| 124 | `Assets/Free-FlashLight/Textures` | `Assets/_ThirdParty/Free-FlashLight/Textures` | GUID giữ nguyên |
| 125 | `Assets/Ghost` | `Assets/_Project/Image/Ghost` | ⚠️ `.meta` bị cấp GUID mới |
| 126 | `Assets/Image` | `Assets/_Project/Image` | GUID giữ nguyên |
| 127 | `Assets/Image/Art` | `Assets/_Project/Image/Art` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 128 | `Assets/Image/Background` | `Assets/_Project/Image/Background` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 129 | `Assets/Image/Misc` | `Assets/_Project/Image/Misc` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 130 | `Assets/Image/PIcture` | `Assets/_Project/Image/PIcture` | GUID giữ nguyên |
| 131 | `Assets/Image/Texture` | `Assets/_Project/Image/Texture` | GUID giữ nguyên |
| 132 | `Assets/MACon` | `Assets/_Project/MACon` | GUID giữ nguyên |
| 133 | `Assets/MACon/Materials` | `Assets/_Project/MACon/Materials` | GUID giữ nguyên |
| 134 | `Assets/MaConBay` | `Assets/_Project/MaConBay` | GUID giữ nguyên |
| 135 | `Assets/MaConBay/Materials` | `Assets/_Project/MaConBay/Materials` | GUID giữ nguyên |
| 136 | `Assets/MaConBay/music box` | `Assets/_Project/MaConBay/music box` | GUID giữ nguyên |
| 137 | `Assets/MaConBay/music box/Materials` | `Assets/_Project/MaConBay/music box/Materials` | GUID giữ nguyên |
| 138 | `Assets/Mame` | `Assets/_Project/Mame` | GUID giữ nguyên |
| 139 | `Assets/MAMe 1` | `Assets/_Project/MAMe 1` | GUID giữ nguyên |
| 140 | `Assets/MAMe 1/Materials` | `Assets/_Project/MAMe 1/Materials` | GUID giữ nguyên |
| 141 | `Assets/Mame/source` | `Assets/_Project/Mame/source` | GUID giữ nguyên |
| 142 | `Assets/Mame/source/Womann` | `Assets/_Project/Mame/source/Womann` | GUID giữ nguyên |
| 143 | `Assets/Mame/textures` | `Assets/_Project/Mame/textures` | GUID giữ nguyên |
| 144 | `Assets/Materials` | `Assets/_Project/Materials` | GUID giữ nguyên |
| 145 | `Assets/Nen` | `Assets/_Project/Nen` | GUID giữ nguyên |
| 146 | `Assets/Nen/Materials` | `Assets/_Project/Nen/Materials` | GUID giữ nguyên |
| 147 | `Assets/OngLam` | `Assets/_Project/OngLam` | GUID giữ nguyên |
| 148 | `Assets/Player` | `Assets/_Project/Player` | GUID giữ nguyên |
| 149 | `Assets/Player/Materials` | `Assets/_Project/Player/Materials` | GUID giữ nguyên; **thư mục đích rỗng đã xoá** (mục 9.2) |
| 150 | `Assets/Quantai` | `Assets/_Project/Quantai` | GUID giữ nguyên |
| 151 | `Assets/Rain` | `Assets/_Project/Rain` | GUID giữ nguyên |
| 152 | `Assets/Rain/Prefab` | `Assets/_Project/Rain/Prefab` | GUID giữ nguyên |
| 153 | `Assets/RoomAudio` | `Assets/_Project/RoomAudio` | GUID giữ nguyên |
| 154 | `Assets/Sky` | `Assets/_ThirdParty/Sky` | GUID giữ nguyên |
| 155 | `Assets/Sky/Cubemaps` | `Assets/_ThirdParty/Sky/Cubemaps` | GUID giữ nguyên |
| 156 | `Assets/Sky/Cubemaps/Classic` | `Assets/_ThirdParty/Sky/Cubemaps/Classic` | GUID giữ nguyên |
| 157 | `Assets/Sky/Panoramics` | `Assets/_ThirdParty/Sky/Panoramics` | GUID giữ nguyên |
| 158 | `Assets/Sky/Panoramics/FS002` | `Assets/_ThirdParty/Sky/Panoramics/FS002` | GUID giữ nguyên |
| 159 | `Assets/Sky/Panoramics/FS003` | `Assets/_ThirdParty/Sky/Panoramics/FS003` | GUID giữ nguyên |
| 160 | `Assets/Sky/Panoramics/FS013` | `Assets/_ThirdParty/Sky/Panoramics/FS013` | GUID giữ nguyên |
| 161 | `Assets/Sky/Panoramics/FS017` | `Assets/_ThirdParty/Sky/Panoramics/FS017` | GUID giữ nguyên |
| 162 | `Assets/Studio` | `Assets/_Project/Studio` | GUID giữ nguyên |
| 163 | `Assets/Tree` | `Assets/_ThirdParty/Tree` | GUID giữ nguyên |
| 164 | `Assets/Tuong` | `Assets/_Project/Tuong` | GUID giữ nguyên |
| 165 | `Assets/Tuong/Materials` | `Assets/_Project/Tuong/Materials` | GUID giữ nguyên |

## 3. File được đổi tên (rename thật sự)

Đây là các file **đổi cả tên file**, không chỉ đổi thư mục. Tổng: **20** file.

| # | Đường dẫn cũ | Đường dẫn mới | Ghi chú |
| ---: | --- | --- | --- |
| 1 | `Assets/BoXuong/internal_ground_ao_texture.jpeg` | `Assets/_Project/Image/Texture/Texture_Skeleton.jpeg` | Rename |
| 2 | `Assets/BoXuong/skeleton_laidout.fbx` | `Assets/_Project/Prefabs/Model/Skeleton.fbx` | Rename |
| 3 | `Assets/Ghost/DOE Ghost.fbx` | `Assets/_Project/Prefabs/Model/Ghost.fbx` | Rename |
| 4 | `Assets/HopSo/Explode 2.fbx` | `Assets/_Project/Prefabs/Model/Skull.fbx` | Rename |
| 5 | `Assets/HopSo/New Material.mat` | `Assets/_Project/Materials/Skull/Skull.mat` | **Rename + sửa nội dung** |
| 6 | `Assets/Image/Texture/5ebeda43-16f2-4723-8068-e6fda9a9efe9.png` | `Assets/_Project/Image/Texture/Bagua.png` | Rename |
| 7 | `Assets/Image/Texture/9447cb8a-45c5-4c85-b8e0-b321d3106b69-fotor-bg-remover-20261019204518.png` | `Assets/_Project/Image/Texture/Graffiti/Graffiti_1.png` | Rename |
| 8 | `Assets/Image/Texture/bua.png` | `Assets/_Project/Image/Texture/Fulu/Fulu_Wall.png` | Rename |
| 9 | `Assets/Image/Texture/buato.png` | `Assets/_Project/Image/Texture/Fulu/Fulu_Single_1.png` | Rename |
| 10 | `Assets/Image/Texture/buato1.png` | `Assets/_Project/Image/Texture/Fulu/Fulu_Single_2.png` | Rename |
| 11 | `Assets/Image/Texture/exec-9b871a8b-9def-426e-8edd-e51c3973ddad.png` | `Assets/_Project/Image/Texture/Graffiti/Graffiti_2.png` | Rename |
| 12 | `Assets/Image/Texture/goi y.png` | `Assets/_Project/Image/Texture/Graffiti/Graffiti_3.png` | Rename |
| 13 | `Assets/Image/Texture/image-fotor-bg-remover-2026101920043.png` | `Assets/_Project/Image/Texture/Graffiti/Graffiti_4.png` | Rename |
| 14 | `Assets/Image/Texture/mau.png` | `Assets/_Project/Image/Texture/Blood/Blood_1.png` | Rename |
| 15 | `Assets/Image/Texture/mau1.png` | `Assets/_Project/Image/Texture/Blood/Blood_2.png` | Rename |
| 16 | `Assets/Image/Texture/vong.png` | `Assets/_Project/Image/Texture/Spell_Texture_Rice.png` | Rename |
| 17 | `Assets/Player/body.jpeg` | `Assets/_Project/Image/Texture/Player/Player_Texture_Body.jpeg` | Rename |
| 18 | `Assets/Player/male3dscan.fbx` | `Assets/_Project/Prefabs/Model/Player.fbx` | Rename |
| 19 | `Assets/Player/Materials/body.mat` | `Assets/_Project/Materials/Player_Body.mat` | **Rename + sửa nội dung** |
| 20 | `Assets/Tree/bca9eaed0a17233bdf19af2dec67c1e5.jpeg` | `Assets/_ThirdParty/Tree/TrreeBark_Texture.jpeg` | Rename |

GUID của tất cả các file trên đều **giữ nguyên** (trừ trường hợp `.meta` thư mục ở mục 9.1).

## 4. File đổi tên kèm chỉnh sửa nội dung

Đây là các trường hợp **không phải file mới**, chỉ là rename và có chỉnh một chút nội dung:

| # | File cũ | File mới | Nội dung thay đổi |
| ---: | --- | --- | --- |
| 1 | `Assets/Player/Materials/body.mat` | `Assets/_Project/Materials/Player_Body.mat` | `m_Name`: `body` → `Player_Body`; bỏ BOM đầu file |
| 2 | `Assets/HopSo/New Material.mat` | `Assets/_Project/Materials/Skull/Skull.mat` | `m_Name`: `New Material` → `Skull`; bỏ BOM đầu file |
| 3 | `Assets/Player/male3dscan.fbx.meta` | `Assets/_Project/Prefabs/Model/Player.fbx.meta` | Thêm ánh xạ material `0503` → `35de48eb48503134fbe1778b56bc1946` trong `externalObjects`; bỏ BOM |
| 4 | `Assets/MAMe 1/Opening.fbx.meta` | `Assets/_Project/MAMe 1/Opening.fbx.meta` | Thêm ánh xạ material `ZombieGirl_body_Material` → `18aea76e67f7fca449df1906a9dccd75`; bỏ BOM |

> Lưu ý: 2 file `.mat` chỉ đổi `m_Name` cho khớp tên mới, **không đổi shader/thuộc tính**.
> 2 file `.fbx.meta` được gán lại material override — nếu model hiển thị sai material thì kiểm tra 2 GUID ở trên.

## 5. Danh sách đầy đủ file đã di chuyển

Nhóm theo cặp **thư mục cũ → thư mục mới**. Cột "File mới" chỉ ghi khi tên file bị đổi;
dấu `—` nghĩa là **giữ nguyên tên file**, chỉ chuyển thư mục.

### 5.1 `Assets/_0317/Materials` → `Assets/_Project/Materials` (7 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `M_Clock_AgedCream.mat` | — | Di chuyển |
| 2 | `M_Clock_AntiqueBrass.mat` | — | Di chuyển |
| 3 | `M_Clock_BlackIron.mat` | — | Di chuyển |
| 4 | `M_Clock_FrostedGlass.mat` | — | Di chuyển |
| 5 | `M_Clock_OldWood.mat` | — | Di chuyển |
| 6 | `M_Flashlight0317_Body.mat` | — | Di chuyển |
| 7 | `M_Flashlight0317_Lens.mat` | — | Di chuyển |

### 5.2 `Assets/_0317/Prefabs/Environment` → `Assets/_Project/Prefabs/Environment` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `GrandfatherClock_0317.prefab` | — | Di chuyển |

### 5.3 `Assets/_0317/Prefabs/Gameplay` → `Assets/_Project/Prefabs/Gameplay` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `PickupFlashlight_0317.prefab` | — | Di chuyển |

### 5.4 `Assets/_0317/Scripts/Gameplay` → `Assets/_Project/Scripts/Gameplay` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DoorKeyPickup.cs` | — | Di chuyển |
| 2 | `DoorKeyUseController.cs` | — | Di chuyển |
| 3 | `EventVoicePlayer.cs` | — | Di chuyển |
| 4 | `ExclusiveGhostAudio.cs` | — | Di chuyển |
| 5 | `Flashlight0317Controller.cs` | — | Di chuyển |
| 6 | `Flashlight0317Pickup.cs` | — | Di chuyển |
| 7 | `GhostProximityLuot.cs` | — | Di chuyển |
| 8 | `GhostRoomAudioController.cs` | — | Di chuyển |
| 9 | `ShrineLightFlicker.cs` | — | Di chuyển |

### 5.5 `Assets/AnhFull/Materials` → `Assets/_Project/Materials/AnhFull` (12 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `2.mat` | — | Di chuyển |
| 2 | `2mecon.mat` | — | Di chuyển |
| 3 | `3.mat` | — | Di chuyển |
| 4 | `4.mat` | — | Di chuyển |
| 5 | `5.mat` | — | Di chuyển |
| 6 | `5ebeda43-16f2-4723-8068-e6fda9a9efe9.mat` | — | Di chuyển |
| 7 | `6.mat` | — | Di chuyển |
| 8 | `AnhPhongTho.mat` | — | Di chuyển |
| 9 | `lathu1.mat` | — | Di chuyển |
| 10 | `MotherPhoto.mat` | — | Di chuyển |
| 11 | `nha-ma-da-lat-6_1698753546.mat` | — | Di chuyển |
| 12 | `thu7.mat` | — | Di chuyển |

### 5.6 `Assets/Audio` → `Assets/_Project/Audio` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `giong_phongghe.mp3` | — | Di chuyển |
| 2 | `gocua.mp3` | — | Di chuyển |
| 3 | `kinhvo.ogg` | — | Di chuyển |
| 4 | `phong_tranh.mp3` | — | Di chuyển |
| 5 | `roitranh.mp3` | — | Di chuyển |

### 5.7 `Assets/BoXuong` → `Assets/_Project/Image/Texture` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `internal_ground_ao_texture.jpeg` | `Texture_Skeleton.jpeg` | Đổi tên |

### 5.8 `Assets/BoXuong` → `Assets/_Project/Prefabs/Model` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `skeleton_laidout.fbx` | `Skeleton.fbx` | Đổi tên |

### 5.9 `Assets/BuddhistFuneralAltar` → `Assets/_Unknown/BuddhistFuneralAltar` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `240423_Buddha Funeral Altar.fbx` | — | Di chuyển |
| 2 | `M_BuddhistAltar.mat` | — | Di chuyển |
| 3 | `props_.png` | — | Di chuyển |
| 4 | `texture.jpeg` | — | Di chuyển |
| 5 | `Vietnamese altar.fbx` | — | Di chuyển |

### 5.10 `Assets/BuddhistFuneralAltar/textures` → `Assets/_Unknown/BuddhistFuneralAltar/textures` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `texture.jpeg` | — | Di chuyển |

### 5.11 `Assets/bupbe` → `Assets/_Unknown/bupbe` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `model.fbx` | — | Di chuyển |
| 2 | `model.jpg` | — | Di chuyển |
| 3 | `New Material.mat` | — | Di chuyển |

### 5.12 `Assets/CacLoaiBay/3DauAn/eye` → `Assets/_Unknown/CacLoaiBay/3DauAn/eye` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `eye_2.fbx` | — | Di chuyển |
| 2 | `Metal035_2K_Color.jpeg` | — | Di chuyển |
| 3 | `Metal035_2K_Displacement.jpeg` | — | Di chuyển |
| 4 | `Metal035_2K_Roughness.jpeg` | — | Di chuyển |
| 5 | `normal.jpeg` | — | Di chuyển |

### 5.13 `Assets/CacLoaiBay/3DauAn/eye/Materials` → `Assets/_Unknown/CacLoaiBay/3DauAn/eye/Materials` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Metal035_2K_Color.mat` | — | Di chuyển |
| 2 | `Metal035_2K_Displacement.mat` | — | Di chuyển |
| 3 | `Metal035_2K_Roughness.mat` | — | Di chuyển |
| 4 | `normal.mat` | — | Di chuyển |

### 5.14 `Assets/CacLoaiBay/3DauAn/Fire` → `Assets/_Unknown/CacLoaiBay/3DauAn/Fire` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `kf2_firebug_perk_symbol.glb` | — | Di chuyển |

### 5.15 `Assets/CacLoaiBay/3DauAn/Moon` → `Assets/_Unknown/CacLoaiBay/3DauAn/Moon` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `moon.obj` | — | Di chuyển |

### 5.16 `Assets/CacLoaiBay/Bandage` → `Assets/_Unknown/CacLoaiBay/Bandage` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `model.dae` | — | Di chuyển |

### 5.17 `Assets/CacLoaiBay/Bandage/Materials` → `Assets/_Unknown/CacLoaiBay/Bandage/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DefaultMaterial_albedo.mat` | — | Di chuyển |

### 5.18 `Assets/CacLoaiBay/Bandage/textures` → `Assets/_Unknown/CacLoaiBay/Bandage/textures` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DefaultMaterial_albedo.jpg` | — | Di chuyển |
| 2 | `DefaultMaterial_AO.jpg` | — | Di chuyển |
| 3 | `DefaultMaterial_metallic.jpg` | — | Di chuyển |
| 4 | `DefaultMaterial_normal.png` | — | Di chuyển |
| 5 | `DefaultMaterial_roughness.jpg` | — | Di chuyển |

### 5.19 `Assets/CacLoaiBay/BayGau` → `Assets/_Unknown/CacLoaiBay/BayGau` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `beartrap.fbx` | — | Di chuyển |
| 2 | `BearTrap_SnapShut.fbx` | — | Di chuyển |

### 5.20 `Assets/CacLoaiBay/BayGau/beartrap` → `Assets/_Unknown/CacLoaiBay/BayGau/beartrap` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Zyklus14_Zeile10_basecolor.JPEG` | — | Di chuyển |
| 2 | `Zyklus14_Zeile10_metallic.JPEG` | — | Di chuyển |
| 3 | `Zyklus14_Zeile10_normal.JPEG` | — | Di chuyển |
| 4 | `Zyklus14_Zeile10_rm.JPEG` | — | Di chuyển |
| 5 | `Zyklus14_Zeile10_roughness.JPEG` | — | Di chuyển |

### 5.21 `Assets/Dau_ThienLinh` → `Assets/_Project/Image/Dau_ThienLinh` (14 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Crâne_AO.png` | — | Di chuyển |
| 2 | `Crâne_BaseColor.png` | — | Di chuyển |
| 3 | `Crâne_Height.png` | — | Di chuyển |
| 4 | `crane_low.fbx` | — | Di chuyển |
| 5 | `Crâne_Metallic.png` | — | Di chuyển |
| 6 | `Crâne_Normal.png` | — | Di chuyển |
| 7 | `Crâne_Roughness.png` | — | Di chuyển |
| 8 | `Dents_AO.png` | — | Di chuyển |
| 9 | `Dents_BaseColor.png` | — | Di chuyển |
| 10 | `Dents_Height.png` | — | Di chuyển |
| 11 | `Dents_Metallic.png` | — | Di chuyển |
| 12 | `Dents_Normal.png` | — | Di chuyển |
| 13 | `Dents_Roughness.png` | — | Di chuyển |
| 14 | `internal_ground_ao_texture.jpeg` | — | Di chuyển |

### 5.22 `Assets/Dau_ThienLinh/Materials` → `Assets/_Project/Materials/Dau_ThienLinh` (7 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Crâne_AO.mat` | — | Di chuyển |
| 2 | `Crâne_BaseColor.mat` | — | Di chuyển |
| 3 | `Crâne_Height.mat` | — | Di chuyển |
| 4 | `Crâne_Metallic.mat` | — | Di chuyển |
| 5 | `Dents_BaseColor.mat` | — | Di chuyển |
| 6 | `Dents_Roughness.mat` | — | Di chuyển |
| 7 | `internal_ground_ao_texture.mat` | — | Di chuyển |

### 5.23 `Assets/DiaryImages` → `Assets/_Project/Image/DiaryImages` (13 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ChatGPT Image Oct 27, 2026, 12_50_43 PM.png` | — | Di chuyển |
| 2 | `ChatGPT Image Oct 28, 2026, 12_44_47 PM-Picsart-BackgroundRemover 1.png` | — | Di chuyển |
| 3 | `ChatGPT Image Oct 28, 2026, 12_44_47 PM-Picsart-BackgroundRemover.png` | — | Di chuyển |
| 4 | `Diary_01 1.png` | — | Di chuyển |
| 5 | `giay1.png` | — | Di chuyển |
| 6 | `Letter_Clock_Warning.png` | — | Di chuyển |
| 7 | `Letter_Daughter_Dolls.png` | — | Di chuyển |
| 8 | `Letter_Daughter_Dolls_v2.png` | — | Di chuyển |
| 9 | `Letter_Mother_Warning.png` | — | Di chuyển |
| 10 | `Letter_Suitcase_Photos.png` | — | Di chuyển |
| 11 | `Ritual_Three_Souls.png` | — | Di chuyển |
| 12 | `thu4-Picsart-BackgroundRemover.png` | — | Di chuyển |
| 13 | `thu5-Picsart-BackgroundRemover.png` | — | Di chuyển |

### 5.24 `Assets/DiaryImages/Materials` → `Assets/_Project/Materials/DiaryImages` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ChatGPT Image Oct 28, 2026, 12_44_47 PM-Picsart-BackgroundRemover 1.mat` | — | Di chuyển |
| 2 | `giay1.mat` | — | Di chuyển |
| 3 | `thu4-Picsart-BackgroundRemover.mat` | — | Di chuyển |

### 5.25 `Assets/FpsHorrorKit` → `Assets/_ThirdParty/FpsHorrorKit` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `InputSystem_Actions.inputactions` | — | Di chuyển |
| 2 | `SkyandFogSettingsProfile.asset` | — | Di chuyển |

### 5.26 `Assets/FpsHorrorKit/Animations` → `Assets/_ThirdParty/FpsHorrorKit/Animations` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `BearTrapSnap.controller` | — | Di chuyển |

### 5.27 `Assets/FpsHorrorKit/Audio` → `Assets/_ThirdParty/FpsHorrorKit/Audio` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Npc_3_Age.wav` | — | Di chuyển |
| 2 | `Npc_3_Name.wav` | — | Di chuyển |
| 3 | `switcheB.wav` | — | Di chuyển |

### 5.28 `Assets/FpsHorrorKit/CameraSettings` → `Assets/_ThirdParty/FpsHorrorKit/CameraSettings` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CustomCameraBlends.asset` | — | Di chuyển |
| 2 | `HeadBob.asset` | — | Di chuyển |

### 5.29 `Assets/FpsHorrorKit/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Materials` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `american_football.mat` | — | Di chuyển |
| 2 | `M_Paper.mat` | — | Di chuyển |
| 3 | `PM_CandleFlame.mat` | — | Di chuyển |
| 4 | `Prototip_Floor.mat` | — | Di chuyển |
| 5 | `Prototip_Wall.mat` | — | Di chuyển |

### 5.30 `Assets/FpsHorrorKit/Materials/DecalMaterials` → `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials` (10 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Ay.mat` | — | Di chuyển |
| 2 | `DM_Blood.mat` | — | Di chuyển |
| 3 | `DM_Crack.mat` | — | Di chuyển |
| 4 | `DM_FingerPrint_DarkGrey.mat` | — | Di chuyển |
| 5 | `DM_FingerPrint_LightGrey.mat` | — | Di chuyển |
| 6 | `DM_FootPrint_01.mat` | — | Di chuyển |
| 7 | `DM_ShoePrint_01_L.mat` | — | Di chuyển |
| 8 | `DM_ShoePrint_01_R.mat` | — | Di chuyển |
| 9 | `HandPrint_01.mat` | — | Di chuyển |
| 10 | `TryDecal.mat` | — | Di chuyển |

### 5.31 `Assets/FpsHorrorKit/Materials/DecalMaterials/TextDecals` → `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials/TextDecals` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DM_ISeeYou.mat` | — | Di chuyển |
| 2 | `DM_NotAlone.mat` | — | Di chuyển |
| 3 | `DM_TheyAreWatching.mat` | — | Di chuyển |
| 4 | `DM_TurnBack.mat` | — | Di chuyển |

### 5.32 `Assets/FpsHorrorKit/Materials/ExorcismCircle` → `Assets/_ThirdParty/FpsHorrorKit/Materials/ExorcismCircle` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ExorcismCircle_Blood.mat` | — | Di chuyển |
| 2 | `ExorcismCircle_Chalk.mat` | — | Di chuyển |
| 3 | `ExorcismCircle_Flame.mat` | — | Di chuyển |
| 4 | `ExorcismCircle_Wax.mat` | — | Di chuyển |

### 5.33 `Assets/FpsHorrorKit/Materials/Interactive` → `Assets/_ThirdParty/FpsHorrorKit/Materials/Interactive` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `SuitcaseAgedStrap.mat` | — | Di chuyển |
| 2 | `SuitcaseDial.mat` | — | Di chuyển |
| 3 | `SuitcaseIndicatorLocked.mat` | — | Di chuyển |
| 4 | `SuitcaseInner.mat` | — | Di chuyển |
| 5 | `SuitcaseLeather.mat` | — | Di chuyển |
| 6 | `SuitcasePhotoPlaceholder.mat` | — | Di chuyển |
| 7 | `SuitcaseRust.mat` | — | Di chuyển |
| 8 | `SuitcaseScuff.mat` | — | Di chuyển |
| 9 | `SuitcaseTrim.mat` | — | Di chuyển |

### 5.34 `Assets/FpsHorrorKit/Models/Carpet` → `Assets/_ThirdParty/FpsHorrorKit/Models/Carpet` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Carpet_2x2.fbx` | — | Di chuyển |
| 2 | `Carpet_2x24.fbx` | — | Di chuyển |
| 3 | `dirty_carpet_diff_2k.png` | — | Di chuyển |
| 4 | `dirty_carpet_nor_gl_2k.png` | — | Di chuyển |
| 5 | `M_Carpet.mat` | — | Di chuyển |

### 5.35 `Assets/FpsHorrorKit/Models/CellarWalls` → `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `cellar_wall_0.5x3.fbx` | — | Di chuyển |
| 2 | `cellar_wall_1x3.fbx` | — | Di chuyển |
| 3 | `cellar_wall_2x3.fbx` | — | Di chuyển |
| 4 | `cellar_wall_3x3.fbx` | — | Di chuyển |

### 5.36 `Assets/FpsHorrorKit/Models/CellarWalls/MatAndTex` → `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls/MatAndTex` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Bodrum_M.mat` | — | Di chuyển |
| 2 | `yellow_plaster_diff_2k.png` | — | Di chuyển |
| 3 | `yellow_plaster_nor_gl_2k.exr` | — | Di chuyển |

### 5.37 `Assets/FpsHorrorKit/Models/Doors` → `Assets/_ThirdParty/FpsHorrorKit/Models/Doors` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Broken_Door.fbx` | — | Di chuyển |
| 2 | `Double_Door.fbx` | — | Di chuyển |
| 3 | `GlassDoor.fbx` | — | Di chuyển |
| 4 | `GothicDoor.fbx` | — | Di chuyển |
| 5 | `OutDoor.fbx` | — | Di chuyển |
| 6 | `Single_Door.fbx` | — | Di chuyển |

### 5.38 `Assets/FpsHorrorKit/Models/Doors/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Materials` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DD_Pattern_M.mat` | — | Di chuyển |
| 2 | `Double_Door_M.mat` | — | Di chuyển |
| 3 | `Glass_Dark.mat` | — | Di chuyển |
| 4 | `Glass_Light.mat` | — | Di chuyển |
| 5 | `GothicDoor.mat` | — | Di chuyển |
| 6 | `M_DoorBody.mat` | — | Di chuyển |
| 7 | `M_DoorHandle.mat` | — | Di chuyển |
| 8 | `MetalDoor.mat` | — | Di chuyển |

### 5.39 `Assets/FpsHorrorKit/Models/Doors/Textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Textures` (11 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `bark_willow_ao_2k.png` | — | Di chuyển |
| 2 | `bark_willow_diff_2k.png` | — | Di chuyển |
| 3 | `bark_willow_nor_gl_2k.png` | — | Di chuyển |
| 4 | `bark_willow_rough_2k.png` | — | Di chuyển |
| 5 | `GothicDoorDiffiuse.png` | — | Di chuyển |
| 6 | `GothicDoorNormal.png` | — | Di chuyển |
| 7 | `metal_plate_02_diff_2k.png` | — | Di chuyển |
| 8 | `metal_plate_02_nor_gl_2k.png` | — | Di chuyển |
| 9 | `rusty_metal_grid_diff_2k.png` | — | Di chuyển |
| 10 | `rusty_metal_grid_nor_gl_2k.exr` | — | Di chuyển |
| 11 | `upscalemedia-transformed.png` | — | Di chuyển |

### 5.40 `Assets/FpsHorrorKit/Models/Floors` → `Assets/_ThirdParty/FpsHorrorKit/Models/Floors` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Floor_0,75x3.fbx` | — | Di chuyển |
| 2 | `Floor_1,75x3.fbx` | — | Di chuyển |
| 3 | `Floor_3x3.fbx` | — | Di chuyển |

### 5.41 `Assets/FpsHorrorKit/Models/Furnitures` → `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures` (15 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ArmChair.fbx` | — | Di chuyển |
| 2 | `chinese_screen_panels.fbx` | — | Di chuyển |
| 3 | `ClassicConsole.fbx` | — | Di chuyển |
| 4 | `Desk.fbx` | — | Di chuyển |
| 5 | `DiningTable.fbx` | — | Di chuyển |
| 6 | `GothicBed.fbx` | — | Di chuyển |
| 7 | `GothicCabinet.fbx` | — | Di chuyển |
| 8 | `GothicCommode.fbx` | — | Di chuyển |
| 9 | `GreenChair.fbx` | — | Di chuyển |
| 10 | `Rockingchair.fbx` | — | Di chuyển |
| 11 | `Sofa_02_Seat.fbx` | — | Di chuyển |
| 12 | `sofa_03_2k.fbx` | — | Di chuyển |
| 13 | `Vintage_Grantfather_Clock.fbx` | — | Di chuyển |
| 14 | `VintageCabinet.fbx` | — | Di chuyển |
| 15 | `WoodenTable.fbx` | — | Di chuyển |

### 5.42 `Assets/FpsHorrorKit/Models/Furnitures/Lights` → `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Lights` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Chandelier.fbx` | — | Di chuyển |
| 2 | `lantern_chandelier 1.fbx` | — | Di chuyển |

### 5.43 `Assets/FpsHorrorKit/Models/Furnitures/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Materials` (25 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `chinese_screen_panels.mat` | — | Di chuyển |
| 2 | `ClassicConsole_01.mat` | — | Di chuyển |
| 3 | `GothicCabinet_01.mat` | — | Di chuyển |
| 4 | `M_ArmChair_Legs.mat` | — | Di chuyển |
| 5 | `M_ArmChair_Pillow.mat` | — | Di chuyển |
| 6 | `M_Chandelier_02.mat` | — | Di chuyển |
| 7 | `M_Chandelier_02_bulb.mat` | — | Di chuyển |
| 8 | `M_Desk.mat` | — | Di chuyển |
| 9 | `M_DiningTable.mat` | — | Di chuyển |
| 10 | `M_GothicBed.mat` | — | Di chuyển |
| 11 | `M_GothicCommode.mat` | — | Di chuyển |
| 12 | `M_GreenChair.mat` | — | Di chuyển |
| 13 | `M_lantern_chandelier_01.mat` | — | Di chuyển |
| 14 | `M_lantern_chandelier_01_glass.mat` | — | Di chuyển |
| 15 | `M_lantern_chandelier_01_lamp.mat` | — | Di chuyển |
| 16 | `M_Rockingchair_01.mat` | — | Di chuyển |
| 17 | `M_Sofa_03.mat` | — | Di chuyển |
| 18 | `M_Sofa_03_fringe.mat` | — | Di chuyển |
| 19 | `M_vintage_cabinet_01_a.mat` | — | Di chuyển |
| 20 | `M_vintage_cabinet_01_b.mat` | — | Di chuyển |
| 21 | `M_vintage_cabinet_01_glass.mat` | — | Di chuyển |
| 22 | `M_vintage_grandfather_clock_01.mat` | — | Di chuyển |
| 23 | `M_vintage_grandfather_clock_01_glass.mat` | — | Di chuyển |
| 24 | `M_WoodenTable.mat` | — | Di chuyển |
| 25 | `sofa_02.mat` | — | Di chuyển |

### 5.44 `Assets/FpsHorrorKit/Models/Furnitures/Textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Textures` (85 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Chandelier_02_diff_2k.png` | — | Di chuyển |
| 2 | `Chandelier_02_metallic_2k.exr` | — | Di chuyển |
| 3 | `Chandelier_02_nor_gl_2k.exr` | — | Di chuyển |
| 4 | `Chandelier_02_nor_gl_2k.png` | — | Di chuyển |
| 5 | `chinese_screen_panels_diff_2k.png` | — | Di chuyển |
| 6 | `chinese_screen_panels_metal_2k.exr` | — | Di chuyển |
| 7 | `chinese_screen_panels_nor_gl_2k.exr` | — | Di chuyển |
| 8 | `chinese_screen_panels_rough_2k.exr` | — | Di chuyển |
| 9 | `chinese_tea_table_diff_2k.png` | — | Di chuyển |
| 10 | `chinese_tea_table_nor_gl_2k.exr` | — | Di chuyển |
| 11 | `chinese_tea_table_rough_2k.exr` | — | Di chuyển |
| 12 | `ClassicConsole_01_diff_2k.png` | — | Di chuyển |
| 13 | `ClassicConsole_01_metallic_2k.exr` | — | Di chuyển |
| 14 | `ClassicConsole_01_nor_gl_2k.exr` | — | Di chuyển |
| 15 | `ClassicConsole_01_roughness_2k.png` | — | Di chuyển |
| 16 | `GothicBed_01_diff_2k.png` | — | Di chuyển |
| 17 | `GothicBed_01_metallic_2k.exr` | — | Di chuyển |
| 18 | `GothicBed_01_nor_gl_2k.exr` | — | Di chuyển |
| 19 | `GothicBed_01_roughness_2k.png` | — | Di chuyển |
| 20 | `GothicCabinet_01_diff_2k.png` | — | Di chuyển |
| 21 | `GothicCabinet_01_metallic_2k.exr` | — | Di chuyển |
| 22 | `GothicCabinet_01_nor_gl_2k.exr` | — | Di chuyển |
| 23 | `GothicCabinet_01_roughness_2k.png` | — | Di chuyển |
| 24 | `GothicCommode_01_diff_2k.png` | — | Di chuyển |
| 25 | `GothicCommode_01_metallic_2k.exr` | — | Di chuyển |
| 26 | `GothicCommode_01_nor_gl_2k.exr` | — | Di chuyển |
| 27 | `GothicCommode_01_roughness_2k.png` | — | Di chuyển |
| 28 | `GreenChair_01_diff_2k.png` | — | Di chuyển |
| 29 | `GreenChair_01_metallic_2k.exr` | — | Di chuyển |
| 30 | `GreenChair_01_nor_gl_2k.exr` | — | Di chuyển |
| 31 | `GreenChair_01_roughness_2k.png` | — | Di chuyển |
| 32 | `lantern_chandelier_01_diff_2k.png` | — | Di chuyển |
| 33 | `lantern_chandelier_01_glass_alpha_2k.png` | — | Di chuyển |
| 34 | `lantern_chandelier_01_glass_diff_2k.png` | — | Di chuyển |
| 35 | `lantern_chandelier_01_glass_metal_2k.png` | — | Di chuyển |
| 36 | `lantern_chandelier_01_glass_nor_gl_2k.png` | — | Di chuyển |
| 37 | `lantern_chandelier_01_glass_rough_2k.png` | — | Di chuyển |
| 38 | `lantern_chandelier_01_metal_2k.exr` | — | Di chuyển |
| 39 | `lantern_chandelier_01_nor_gl_2k.exr` | — | Di chuyển |
| 40 | `lantern_chandelier_01_rough_2k.png` | — | Di chuyển |
| 41 | `modern_arm_chair_01_legs_diff_2k.png` | — | Di chuyển |
| 42 | `modern_arm_chair_01_legs_metal_2k.png` | — | Di chuyển |
| 43 | `modern_arm_chair_01_legs_nor_gl_2k.png` | — | Di chuyển |
| 44 | `modern_arm_chair_01_legs_rough_2k.png` | — | Di chuyển |
| 45 | `modern_arm_chair_01_pillow_diff_2k.png` | — | Di chuyển |
| 46 | `modern_arm_chair_01_pillow_metal_2k.png` | — | Di chuyển |
| 47 | `modern_arm_chair_01_pillow_nor_gl_2k.png` | — | Di chuyển |
| 48 | `modern_arm_chair_01_pillow_rough_2k.png` | — | Di chuyển |
| 49 | `modern_coffee_table_01_diff_2k.png` | — | Di chuyển |
| 50 | `modern_coffee_table_01_nor_gl_2k.exr` | — | Di chuyển |
| 51 | `modern_coffee_table_01_rough_2k.exr` | — | Di chuyển |
| 52 | `potted_plant_01_leaves_alpha_2k.png` | — | Di chuyển |
| 53 | `potted_plant_01_leaves_diff_2k.png` | — | Di chuyển |
| 54 | `potted_plant_01_leaves_nor_gl_2k.png` | — | Di chuyển |
| 55 | `potted_plant_01_leaves_rough_2k.png` | — | Di chuyển |
| 56 | `potted_plant_01_pot_diff_2k.png` | — | Di chuyển |
| 57 | `potted_plant_01_pot_nor_gl_2k.png` | — | Di chuyển |
| 58 | `potted_plant_01_pot_rough_2k.png` | — | Di chuyển |
| 59 | `Rockingchair_01_diff_2k.png` | — | Di chuyển |
| 60 | `Rockingchair_01_metallic_2k.exr` | — | Di chuyển |
| 61 | `Rockingchair_01_nor_gl_2k.exr` | — | Di chuyển |
| 62 | `Rockingchair_01_roughness_2k.png` | — | Di chuyển |
| 63 | `sofa_02_diff_2k.png` | — | Di chuyển |
| 64 | `sofa_02_metallic_2k.exr` | — | Di chuyển |
| 65 | `sofa_02_nor_gl_2k.exr` | — | Di chuyển |
| 66 | `sofa_02_roughness_2k.png` | — | Di chuyển |
| 67 | `sofa_03_diff_2k.png` | — | Di chuyển |
| 68 | `sofa_03_nor_gl_2k.exr` | — | Di chuyển |
| 69 | `sofa_03_opacity_2k.png` | — | Di chuyển |
| 70 | `sofa_03_rough_2k.png` | — | Di chuyển |
| 71 | `vintage_cabinet_01_a_diff_2k.png` | — | Di chuyển |
| 72 | `vintage_cabinet_01_a_metal_2k.png` | — | Di chuyển |
| 73 | `vintage_cabinet_01_a_nor_gl_2k.png` | — | Di chuyển |
| 74 | `vintage_cabinet_01_a_rough_2k.png` | — | Di chuyển |
| 75 | `vintage_cabinet_01_b_diff_2k.png` | — | Di chuyển |
| 76 | `vintage_cabinet_01_b_nor_gl_2k.png` | — | Di chuyển |
| 77 | `vintage_cabinet_01_b_rough_2k.png` | — | Di chuyển |
| 78 | `vintage_grandfather_clock_01_diff_2k.png` | — | Di chuyển |
| 79 | `vintage_grandfather_clock_01_metal_2k.exr` | — | Di chuyển |
| 80 | `vintage_grandfather_clock_01_nor_gl_2k.exr` | — | Di chuyển |
| 81 | `vintage_grandfather_clock_01_rough_2k.exr` | — | Di chuyển |
| 82 | `WoodenTable_01_diff_2k.png` | — | Di chuyển |
| 83 | `WoodenTable_01_metallic_2k.exr` | — | Di chuyển |
| 84 | `WoodenTable_01_nor_gl_2k.exr` | — | Di chuyển |
| 85 | `WoodenTable_01_roughness_2k.png` | — | Di chuyển |

### 5.45 `Assets/FpsHorrorKit/Models/ModulerParts` → `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Column.fbx` | — | Di chuyển |
| 2 | `Curtain_2x3.fbx` | — | Di chuyển |
| 3 | `Wall_3x1.fbx` | — | Di chuyển |
| 4 | `Wall_3x1_DM.fbx` | — | Di chuyển |
| 5 | `Wall_3x3.fbx` | — | Di chuyển |
| 6 | `Wall_3x3_DM.fbx` | — | Di chuyển |
| 7 | `Wall_Window_2x3 1.fbx` | — | Di chuyển |
| 8 | `Wall_Window_3x3.fbx` | — | Di chuyển |

### 5.46 `Assets/FpsHorrorKit/Models/ModulerParts/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Materials` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `M_Curtain.mat` | — | Di chuyển |
| 2 | `Pipe.mat` | — | Di chuyển |

### 5.47 `Assets/FpsHorrorKit/Models/ModulerParts/Textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Textures` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `fabric_pattern_07_col_2_2k.png` | — | Di chuyển |
| 2 | `fabric_pattern_07_nor_gl_2k.png` | — | Di chuyển |

### 5.48 `Assets/FpsHorrorKit/Models/NewModels` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `LanternFuel.fbx` | — | Di chuyển |
| 2 | `m_body.mat` | — | Di chuyển |
| 3 | `m_frame.mat` | — | Di chuyển |
| 4 | `m_text.mat` | — | Di chuyển |

### 5.49 `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Camera_01_2k 1.fbx` | — | Di chuyển |

### 5.50 `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` (12 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Camera_01_body_diff_2k.png` | — | Di chuyển |
| 2 | `Camera_01_body_metallic_2k.png` | — | Di chuyển |
| 3 | `Camera_01_body_nor_gl_2k.png` | — | Di chuyển |
| 4 | `Camera_01_body_roughness_2k.png` | — | Di chuyển |
| 5 | `Camera_01_lens_body_diff_2k.png` | — | Di chuyển |
| 6 | `Camera_01_lens_body_metallic_2k.png` | — | Di chuyển |
| 7 | `Camera_01_lens_body_nor_gl_2k.png` | — | Di chuyển |
| 8 | `Camera_01_lens_body_roughness_2k.png` | — | Di chuyển |
| 9 | `Camera_01_strap_diff_2k.png` | — | Di chuyển |
| 10 | `Camera_01_strap_metallic_2k.png` | — | Di chuyển |
| 11 | `Camera_01_strap_nor_gl_2k.png` | — | Di chuyển |
| 12 | `Camera_01_strap_roughness_2k.png` | — | Di chuyển |

### 5.51 `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Lantern_01_2k.fbx` | — | Di chuyển |
| 2 | `Lantern_01_brass.mat` | — | Di chuyển |
| 3 | `Lantern_01_glass.mat` | — | Di chuyển |

### 5.52 `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Lantern_01_brass_diff_2k.png` | — | Di chuyển |
| 2 | `Lantern_01_brass_metallic_2k.png` | — | Di chuyển |
| 3 | `Lantern_01_brass_nor_gl_2k.png` | — | Di chuyển |
| 4 | `Lantern_01_brass_opacity_2k.png` | — | Di chuyển |
| 5 | `Lantern_01_brass_roughness_2k.png` | — | Di chuyển |

### 5.53 `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `modular_industrial_pipes.fbx` | — | Di chuyển |

### 5.54 `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `modular_industrial_pipes_01_group01.mat` | — | Di chuyển |
| 2 | `modular_industrial_pipes_01_group02.mat` | — | Di chuyển |

### 5.55 `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `modular_industrial_pipes_01_pipe_a.prefab` | — | Di chuyển |
| 2 | `modular_industrial_pipes_01_pipe_b.prefab` | — | Di chuyển |
| 3 | `modular_industrial_pipes_01_pipe_c.prefab` | — | Di chuyển |
| 4 | `modular_industrial_pipes_01_pipe_d.prefab` | — | Di chuyển |
| 5 | `modular_industrial_pipes_01_pipe_e.prefab` | — | Di chuyển |
| 6 | `modular_industrial_pipes_01_pipe_f.prefab` | — | Di chuyển |
| 7 | `modular_industrial_pipes_01_pipe_g.prefab` | — | Di chuyển |
| 8 | `modular_industrial_pipes_01_pipe_h.prefab` | — | Di chuyển |

### 5.56 `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `modular_industrial_pipes_01_group01_diff_2k.png` | — | Di chuyển |
| 2 | `modular_industrial_pipes_01_group01_metal_2k.png` | — | Di chuyển |
| 3 | `modular_industrial_pipes_01_group01_nor_gl_2k.png` | — | Di chuyển |
| 4 | `modular_industrial_pipes_01_group01_rough_2k.png` | — | Di chuyển |
| 5 | `modular_industrial_pipes_01_group02_diff_2k.png` | — | Di chuyển |
| 6 | `modular_industrial_pipes_01_group02_metal_2k.png` | — | Di chuyển |
| 7 | `modular_industrial_pipes_01_group02_nor_gl_2k.png` | — | Di chuyển |
| 8 | `modular_industrial_pipes_01_group02_rough_2k.png` | — | Di chuyển |

### 5.57 `Assets/FpsHorrorKit/Models/NewModels/textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/textures` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `rusty_metal_sheet_arm_2k.png` | — | Di chuyển |
| 2 | `rusty_metal_sheet_diff_2k.png` | — | Di chuyển |
| 3 | `rusty_metal_sheet_nor_gl_2k.png` | — | Di chuyển |

### 5.58 `Assets/FpsHorrorKit/Models/NewModels/UtilityBox` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `utility_box_01.mat` | — | Di chuyển |
| 2 | `UtilityBox.fbx` | — | Di chuyển |

### 5.59 `Assets/FpsHorrorKit/Models/NewModels/UtilityBox/textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox/textures` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `utility_box_01_diff_2k.png` | — | Di chuyển |
| 2 | `utility_box_01_metal_2k.exr` | — | Di chuyển |
| 3 | `utility_box_01_nor_gl_2k.exr` | — | Di chuyển |
| 4 | `utility_box_01_rough_2k.exr` | — | Di chuyển |

### 5.60 `Assets/FpsHorrorKit/Models/Props` → `Assets/_ThirdParty/FpsHorrorKit/Models/Props` (28 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `american_football.fbx` | — | Di chuyển |
| 2 | `brass_candleholder_a.fbx` | — | Di chuyển |
| 3 | `brass_candleholder_b.fbx` | — | Di chuyển |
| 4 | `brass_candleholder_c.fbx` | — | Di chuyển |
| 5 | `brass_goblet_a.fbx` | — | Di chuyển |
| 6 | `brass_goblet_b.fbx` | — | Di chuyển |
| 7 | `brass_goblet_c.fbx` | — | Di chuyển |
| 8 | `brass_pot_a.fbx` | — | Di chuyển |
| 9 | `brass_pot_b.fbx` | — | Di chuyển |
| 10 | `brass_vase_a.fbx` | — | Di chuyển |
| 11 | `brass_vase_b.fbx` | — | Di chuyển |
| 12 | `brass_vase_c.fbx` | — | Di chuyển |
| 13 | `Cardboard_Box.fbx` | — | Di chuyển |
| 14 | `Carved_Wooden_Elephant.fbx` | — | Di chuyển |
| 15 | `carved_wooden_plate.fbx` | — | Di chuyển |
| 16 | `fancy_picture_frame_01.fbx` | — | Di chuyển |
| 17 | `fancy_picture_frame_02.fbx` | — | Di chuyển |
| 18 | `hanging_picture_frame_03.fbx` | — | Di chuyển |
| 19 | `LightSiwtcher.fbx` | — | Di chuyển |
| 20 | `PaperMesh.fbx` | — | Di chuyển |
| 21 | `potted_plant_01.fbx` | — | Di chuyển |
| 22 | `power_box_01_2k_Denek2.fbx` | — | Di chuyển |
| 23 | `vintage_pocket_watch.fbx` | — | Di chuyển |
| 24 | `wine_bottles_01_alsace.fbx` | — | Di chuyển |
| 25 | `wine_bottles_01_bordeaux.fbx` | — | Di chuyển |
| 26 | `wine_bottles_01_burgundy.fbx` | — | Di chuyển |
| 27 | `wine_bottles_01_champagne.fbx` | — | Di chuyển |
| 28 | `wooden_bowl_02.fbx` | — | Di chuyển |

### 5.61 `Assets/FpsHorrorKit/Models/Props/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Materials` (44 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Button_M.mat` | — | Di chuyển |
| 2 | `cardboard_box_01.mat` | — | Di chuyển |
| 3 | `fancy_picture_frame_01.mat` | — | Di chuyển |
| 4 | `fancy_picture_frame_01_canvas.mat` | — | Di chuyển |
| 5 | `fancy_picture_frame_02.mat` | — | Di chuyển |
| 6 | `fancy_picture_frame_02_canvas.mat` | — | Di chuyển |
| 7 | `hanging_picture_frame_03.mat` | — | Di chuyển |
| 8 | `hanging_picture_frame_03_artwork.mat` | — | Di chuyển |
| 9 | `hanging_picture_frame_03_glass.mat` | — | Di chuyển |
| 10 | `M_brass_candleholders_01.mat` | — | Di chuyển |
| 11 | `M_brass_candleholders_02.mat` | — | Di chuyển |
| 12 | `M_brass_candleholders_03.mat` | — | Di chuyển |
| 13 | `M_brass_candleholders_candles.mat` | — | Di chuyển |
| 14 | `M_brass_candleholders_flame.mat` | — | Di chuyển |
| 15 | `M_brass_goblets_01.mat` | — | Di chuyển |
| 16 | `M_brass_goblets_02.mat` | — | Di chuyển |
| 17 | `M_brass_goblets_03.mat` | — | Di chuyển |
| 18 | `M_brass_pot_01.mat` | — | Di chuyển |
| 19 | `M_brass_pot_02.mat` | — | Di chuyển |
| 20 | `M_brass_vase_001.mat` | — | Di chuyển |
| 21 | `M_brass_vase_02.mat` | — | Di chuyển |
| 22 | `M_brass_vase_03.mat` | — | Di chuyển |
| 23 | `M_carved_wooden_elephant.001.mat` | — | Di chuyển |
| 24 | `M_carved_wooden_plate.mat` | — | Di chuyển |
| 25 | `M_vintage_pocket_watch.mat` | — | Di chuyển |
| 26 | `M_vintage_pocket_watch_glass.mat` | — | Di chuyển |
| 27 | `M_vintage_pocket_watch_hands.mat` | — | Di chuyển |
| 28 | `M_wooden_bowl_02.mat` | — | Di chuyển |
| 29 | `potted_plant_01_leaves.mat` | — | Di chuyển |
| 30 | `potted_plant_01_pot.mat` | — | Di chuyển |
| 31 | `power_box_01.mat` | — | Di chuyển |
| 32 | `White Plastic.mat` | — | Di chuyển |
| 33 | `wine_bottles_01_clearwater.mat` | — | Di chuyển |
| 34 | `wine_bottles_01_ftero_vinea_01.mat` | — | Di chuyển |
| 35 | `wine_bottles_01_ftero_vinea_02.mat` | — | Di chuyển |
| 36 | `wine_bottles_01_glass_clear.mat` | — | Di chuyển |
| 37 | `wine_bottles_01_glass_green.mat` | — | Di chuyển |
| 38 | `wine_bottles_01_glass_orange.mat` | — | Di chuyển |
| 39 | `wine_bottles_01_glass_yellow.mat` | — | Di chuyển |
| 40 | `wine_bottles_01_intertwine.mat` | — | Di chuyển |
| 41 | `wine_bottles_01_seal_cork.mat` | — | Di chuyển |
| 42 | `wine_bottles_01_wine_clear.mat` | — | Di chuyển |
| 43 | `wine_bottles_01_wine_light_red.mat` | — | Di chuyển |
| 44 | `wine_bottles_01_wine_red.mat` | — | Di chuyển |

### 5.62 `Assets/FpsHorrorKit/Models/Props/Textures` → `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Textures` (115 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `american_football_diff_2k.png` | — | Di chuyển |
| 2 | `american_football_nor_gl_1k.exr` | — | Di chuyển |
| 3 | `american_football_rough_1k.exr` | — | Di chuyển |
| 4 | `brass_candleholders_01_diff_2k.png` | — | Di chuyển |
| 5 | `brass_candleholders_01_metal_2k.png` | — | Di chuyển |
| 6 | `brass_candleholders_01_nor_gl_2k.png` | — | Di chuyển |
| 7 | `brass_candleholders_01_rough_2k.png` | — | Di chuyển |
| 8 | `brass_candleholders_02_diff_2k.png` | — | Di chuyển |
| 9 | `brass_candleholders_02_metal_2k.png` | — | Di chuyển |
| 10 | `brass_candleholders_02_nor_gl_2k.png` | — | Di chuyển |
| 11 | `brass_candleholders_02_rough_2k.png` | — | Di chuyển |
| 12 | `brass_candleholders_03_diff_2k.png` | — | Di chuyển |
| 13 | `brass_candleholders_03_metal_2k.png` | — | Di chuyển |
| 14 | `brass_candleholders_03_nor_gl_2k.png` | — | Di chuyển |
| 15 | `brass_candleholders_03_rough_2k.png` | — | Di chuyển |
| 16 | `brass_candleholders_candles_diff_2k.png` | — | Di chuyển |
| 17 | `brass_candleholders_candles_nor_gl_2k.png` | — | Di chuyển |
| 18 | `brass_candleholders_candles_rough_2k.png` | — | Di chuyển |
| 19 | `brass_candleholders_flame_diff_2k.png` | — | Di chuyển |
| 20 | `brass_goblet_01_diff_2k.png` | — | Di chuyển |
| 21 | `brass_goblet_01_metal_2k.exr` | — | Di chuyển |
| 22 | `brass_goblet_01_nor_gl_2k.exr` | — | Di chuyển |
| 23 | `brass_goblet_01_rough_2k.exr` | — | Di chuyển |
| 24 | `brass_goblet_02_diff_2k.png` | — | Di chuyển |
| 25 | `brass_goblet_02_metal_2k.exr` | — | Di chuyển |
| 26 | `brass_goblet_02_nor_gl_2k.exr` | — | Di chuyển |
| 27 | `brass_goblet_02_rough_2k.exr` | — | Di chuyển |
| 28 | `brass_goblet_03_diff_2k.png` | — | Di chuyển |
| 29 | `brass_goblet_03_metal_2k.exr` | — | Di chuyển |
| 30 | `brass_goblet_03_nor_gl_2k.exr` | — | Di chuyển |
| 31 | `brass_goblet_03_rough_2k.exr` | — | Di chuyển |
| 32 | `brass_pot_01_diff_2k.png` | — | Di chuyển |
| 33 | `brass_pot_01_metal_2k.exr` | — | Di chuyển |
| 34 | `brass_pot_01_nor_gl_2k.exr` | — | Di chuyển |
| 35 | `brass_pot_01_rough_2k.exr` | — | Di chuyển |
| 36 | `brass_pot_02_diff_2k.png` | — | Di chuyển |
| 37 | `brass_pot_02_metal_2k.exr` | — | Di chuyển |
| 38 | `brass_pot_02_nor_gl_2k.exr` | — | Di chuyển |
| 39 | `brass_pot_02_rough_2k.exr` | — | Di chuyển |
| 40 | `brass_vase_01_diff_2k.png` | — | Di chuyển |
| 41 | `brass_vase_01_metal_2k.exr` | — | Di chuyển |
| 42 | `brass_vase_01_nor_gl_2k.exr` | — | Di chuyển |
| 43 | `brass_vase_01_rough_2k.png` | — | Di chuyển |
| 44 | `brass_vase_02_diff_2k.png` | — | Di chuyển |
| 45 | `brass_vase_02_metal_2k.exr` | — | Di chuyển |
| 46 | `brass_vase_02_nor_gl_2k.exr` | — | Di chuyển |
| 47 | `brass_vase_02_rough_2k.exr` | — | Di chuyển |
| 48 | `brass_vase_03_diff_2k.png` | — | Di chuyển |
| 49 | `brass_vase_03_metal_2k.exr` | — | Di chuyển |
| 50 | `brass_vase_03_nor_gl_2k.exr` | — | Di chuyển |
| 51 | `brass_vase_03_rough_2k.exr` | — | Di chuyển |
| 52 | `cardboard_box_01_diff_2k.png` | — | Di chuyển |
| 53 | `cardboard_box_01_nor_gl_2k.exr` | — | Di chuyển |
| 54 | `cardboard_box_01_rough_2k.exr` | — | Di chuyển |
| 55 | `carved_wooden_elephant_diff_2k.png` | — | Di chuyển |
| 56 | `carved_wooden_elephant_nor_gl_2k.exr` | — | Di chuyển |
| 57 | `carved_wooden_elephant_rough_2k.exr` | — | Di chuyển |
| 58 | `carved_wooden_plate_diff_2k.png` | — | Di chuyển |
| 59 | `carved_wooden_plate_nor_gl_2k.exr` | — | Di chuyển |
| 60 | `carved_wooden_plate_rough_2k.exr` | — | Di chuyển |
| 61 | `fancy_picture_frame_01_canvas_diff_2k.png` | — | Di chuyển |
| 62 | `fancy_picture_frame_01_canvas_nor_gl_2k.png` | — | Di chuyển |
| 63 | `fancy_picture_frame_01_canvas_rough_2k.png` | — | Di chuyển |
| 64 | `fancy_picture_frame_01_diff_2k.png` | — | Di chuyển |
| 65 | `fancy_picture_frame_01_nor_gl_2k.exr` | — | Di chuyển |
| 66 | `fancy_picture_frame_01_rough_2k.png` | — | Di chuyển |
| 67 | `fancy_picture_frame_02_canvas_diff_2k.png` | — | Di chuyển |
| 68 | `fancy_picture_frame_02_canvas_nor_gl_2k.png` | — | Di chuyển |
| 69 | `fancy_picture_frame_02_canvas_rough_2k.png` | — | Di chuyển |
| 70 | `fancy_picture_frame_02_diff_2k.png` | — | Di chuyển |
| 71 | `fancy_picture_frame_02_nor_gl_2k.exr` | — | Di chuyển |
| 72 | `fancy_picture_frame_02_rough_2k.png` | — | Di chuyển |
| 73 | `hanging_picture_frame_03_artwork_diff_2k.png` | — | Di chuyển |
| 74 | `hanging_picture_frame_03_artwork_nor_gl_2k.png` | — | Di chuyển |
| 75 | `hanging_picture_frame_03_artwork_rough_2k.png` | — | Di chuyển |
| 76 | `hanging_picture_frame_03_diff_2k.png` | — | Di chuyển |
| 77 | `hanging_picture_frame_03_metal_2k.exr` | — | Di chuyển |
| 78 | `hanging_picture_frame_03_nor_gl_2k.exr` | — | Di chuyển |
| 79 | `hanging_picture_frame_03_rough_2k.png` | — | Di chuyển |
| 80 | `power_box_01_diff_2k.png` | — | Di chuyển |
| 81 | `power_box_01_metal_2k.exr` | — | Di chuyển |
| 82 | `power_box_01_nor_gl_2k.exr` | — | Di chuyển |
| 83 | `power_box_01_rough_2k.exr` | — | Di chuyển |
| 84 | `rusty_metal_02_diff_2k (1).png` | — | Di chuyển |
| 85 | `rusty_metal_sheet_diff_2k.png` | — | Di chuyển |
| 86 | `rusty_metal_sheet_nor_gl_2k.png` | — | Di chuyển |
| 87 | `vintage_pocket_watch_diff_2k.png` | — | Di chuyển |
| 88 | `vintage_pocket_watch_metal_2k.exr` | — | Di chuyển |
| 89 | `vintage_pocket_watch_nor_gl_2k.exr` | — | Di chuyển |
| 90 | `vintage_pocket_watch_rough_2k.exr` | — | Di chuyển |
| 91 | `wine_bottles_01_alpha_2k.png` | — | Di chuyển |
| 92 | `wine_bottles_01_diff_2k.png` | — | Di chuyển |
| 93 | `wine_bottles_01_label_clearwater_crystals_01_alpha_2k.png` | — | Di chuyển |
| 94 | `wine_bottles_01_label_clearwater_crystals_01_diff_2k.png` | — | Di chuyển |
| 95 | `wine_bottles_01_label_clearwater_crystals_01_metal_2k.png` | — | Di chuyển |
| 96 | `wine_bottles_01_label_clearwater_crystals_01_rough_2k.png` | — | Di chuyển |
| 97 | `wine_bottles_01_label_ftero_vinea_01_alpha_2k.png` | — | Di chuyển |
| 98 | `wine_bottles_01_label_ftero_vinea_01_diff_2k.png` | — | Di chuyển |
| 99 | `wine_bottles_01_label_ftero_vinea_01_metal_2k.png` | — | Di chuyển |
| 100 | `wine_bottles_01_label_ftero_vinea_01_rough_2k.png` | — | Di chuyển |
| 101 | `wine_bottles_01_label_ftero_vinea_02_alpha_2k.png` | — | Di chuyển |
| 102 | `wine_bottles_01_label_ftero_vinea_02_diff_2k.png` | — | Di chuyển |
| 103 | `wine_bottles_01_label_ftero_vinea_02_metal_2k.png` | — | Di chuyển |
| 104 | `wine_bottles_01_label_ftero_vinea_02_rough_2k.png` | — | Di chuyển |
| 105 | `wine_bottles_01_label_intertwine_01_alpha_2k.png` | — | Di chuyển |
| 106 | `wine_bottles_01_label_intertwine_01_diff_2k.png` | — | Di chuyển |
| 107 | `wine_bottles_01_label_intertwine_01_metal_2k.png` | — | Di chuyển |
| 108 | `wine_bottles_01_label_intertwine_01_rough_2k.png` | — | Di chuyển |
| 109 | `wine_bottles_01_metal_2k.exr` | — | Di chuyển |
| 110 | `wine_bottles_01_nor_gl_2k.exr` | — | Di chuyển |
| 111 | `wine_bottles_01_paper_nor_gl_2k.png` | — | Di chuyển |
| 112 | `wine_bottles_01_rough_2k.exr` | — | Di chuyển |
| 113 | `wooden_bowl_02_diff_2k.png` | — | Di chuyển |
| 114 | `wooden_bowl_02_nor_gl_2k.exr` | — | Di chuyển |
| 115 | `wooden_bowl_02_rough_2k.exr` | — | Di chuyển |

### 5.63 `Assets/FpsHorrorKit/Models/Stairs` → `Assets/_ThirdParty/FpsHorrorKit/Models/Stairs` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Stairs.fbx` | — | Di chuyển |
| 2 | `Stairs.mat` | — | Di chuyển |
| 3 | `StairsRailing.fbx` | — | Di chuyển |

### 5.64 `Assets/FpsHorrorKit/Png` → `Assets/_ThirdParty/FpsHorrorKit/Png` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CamTry2.png` | — | Di chuyển |
| 2 | `CamTry3.png` | — | Di chuyển |
| 3 | `CamTry4.png` | — | Di chuyển |

### 5.65 `Assets/FpsHorrorKit/Png/Blood` → `Assets/_ThirdParty/FpsHorrorKit/Png/Blood` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Blood_1.png` | — | Di chuyển |
| 2 | `Blood_2.png` | — | Di chuyển |
| 3 | `Blood_3.png` | — | Di chuyển |
| 4 | `Blood_5.png` | — | Di chuyển |
| 5 | `Help.png` | — | Di chuyển |
| 6 | `HelpMe.png` | — | Di chuyển |

### 5.66 `Assets/FpsHorrorKit/Png/Blood/Materials` → `Assets/_ThirdParty/FpsHorrorKit/Png/Blood/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Help.mat` | — | Di chuyển |

### 5.67 `Assets/FpsHorrorKit/Png/Item` → `Assets/_ThirdParty/FpsHorrorKit/Png/Item` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FlashlightIn.png` | — | Di chuyển |
| 2 | `FlashlightOut.png` | — | Di chuyển |

### 5.68 `Assets/FpsHorrorKit/Png/Papers` → `Assets/_ThirdParty/FpsHorrorKit/Png/Papers` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `kagit.png` | — | Di chuyển |
| 2 | `kagit_2.png` | — | Di chuyển |
| 3 | `MyPaper.png` | — | Di chuyển |
| 4 | `paper-1763262_1920.png` | — | Di chuyển |

### 5.69 `Assets/FpsHorrorKit/Png/ScriptIcons` → `Assets/_ThirdParty/FpsHorrorKit/Png/ScriptIcons` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DialogueDataIcon.png` | — | Di chuyển |
| 2 | `DialogueSystemIcon 1.png` | — | Di chuyển |
| 3 | `FpsAssetsInputsIcon.png` | — | Di chuyển |
| 4 | `FPSControllerIcon.png` | — | Di chuyển |

### 5.70 `Assets/FpsHorrorKit/Png/UI` → `Assets/_ThirdParty/FpsHorrorKit/Png/UI` (12 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CameraBattery.png` | — | Di chuyển |
| 2 | `HoldMouseLeft.png` | — | Di chuyển |
| 3 | `HoldMouseLeft_And_DragUpDown.png` | — | Di chuyển |
| 4 | `HoldMouseLeftV2.png` | — | Di chuyển |
| 5 | `InventoryPanel1.png` | — | Di chuyển |
| 6 | `LanternFuel.png` | — | Di chuyển |
| 7 | `LanternIn_v1.png` | — | Di chuyển |
| 8 | `LanternIn_V2.png` | — | Di chuyển |
| 9 | `LanternOut.png` | — | Di chuyển |
| 10 | `LeftClick.png` | — | Di chuyển |
| 11 | `S_Lantern.png` | — | Di chuyển |
| 12 | `S_PhotoCamera.png` | — | Di chuyển |

### 5.71 `Assets/FpsHorrorKit/Prefabs/Cellar` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Cellar` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `cellar_wall_0.5x3.prefab` | — | Di chuyển |
| 2 | `cellar_wall_1x3.prefab` | — | Di chuyển |
| 3 | `cellar_wall_2x3.prefab` | — | Di chuyển |
| 4 | `cellar_wall_3x3.prefab` | — | Di chuyển |

### 5.72 `Assets/FpsHorrorKit/Prefabs/Decals` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Decals` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Fingerprint_DarkGrey.prefab` | — | Di chuyển |
| 2 | `Fingerprint_LightGrey.prefab` | — | Di chuyển |
| 3 | `FootPrint_01_L.prefab` | — | Di chuyển |
| 4 | `FootPrint_01_R.prefab` | — | Di chuyển |
| 5 | `HandPrint_L.prefab` | — | Di chuyển |
| 6 | `HandPrint_R.prefab` | — | Di chuyển |
| 7 | `ShoePrint_01_L.prefab` | — | Di chuyển |
| 8 | `ShoePrint_01_R.prefab` | — | Di chuyển |

### 5.73 `Assets/FpsHorrorKit/Prefabs/Doors` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Doors` (7 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `BreakDoor.prefab` | — | Di chuyển |
| 2 | `Door_Frame.prefab` | — | Di chuyển |
| 3 | `Double_Door.prefab` | — | Di chuyển |
| 4 | `Glass_Door.prefab` | — | Di chuyển |
| 5 | `GothicDoor.prefab` | — | Di chuyển |
| 6 | `OutDoor.prefab` | — | Di chuyển |
| 7 | `Single_Door.prefab` | — | Di chuyển |

### 5.74 `Assets/FpsHorrorKit/Prefabs/Floors` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Floors` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Floor_0,75x3.prefab` | — | Di chuyển |
| 2 | `Floor_1,75x3.prefab` | — | Di chuyển |
| 3 | `Floor_3x3.prefab` | — | Di chuyển |

### 5.75 `Assets/FpsHorrorKit/Prefabs/Furnitures` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures` (18 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ArmChair.prefab` | — | Di chuyển |
| 2 | `Carpet_2x2.prefab` | — | Di chuyển |
| 3 | `Carpet_2x24.prefab` | — | Di chuyển |
| 4 | `chinese_screen_panels.prefab` | — | Di chuyển |
| 5 | `ClassicConsole.prefab` | — | Di chuyển |
| 6 | `Curtain_2x3.prefab` | — | Di chuyển |
| 7 | `Desk.prefab` | — | Di chuyển |
| 8 | `DiningTable.prefab` | — | Di chuyển |
| 9 | `GothicBed.prefab` | — | Di chuyển |
| 10 | `GothicCabinet.prefab` | — | Di chuyển |
| 11 | `GothicCommode.prefab` | — | Di chuyển |
| 12 | `GreenChair.prefab` | — | Di chuyển |
| 13 | `Rockingchair.prefab` | — | Di chuyển |
| 14 | `Sofa_02_Seat.prefab` | — | Di chuyển |
| 15 | `sofa_03_2k.prefab` | — | Di chuyển |
| 16 | `Vintage_Grantfather_Clock.prefab` | — | Di chuyển |
| 17 | `VintageCabinet.prefab` | — | Di chuyển |
| 18 | `WoodenTable.prefab` | — | Di chuyển |

### 5.76 `Assets/FpsHorrorKit/Prefabs/Furnitures/Lights` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures/Lights` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Chandelier.prefab` | — | Di chuyển |
| 2 | `Lantern_Chandelier.prefab` | — | Di chuyển |

### 5.77 `Assets/FpsHorrorKit/Prefabs/InteractablePrefabs` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/InteractablePrefabs` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CameraBattery.prefab` | — | Di chuyển |
| 2 | `ITO_Paper_a.prefab` | — | Di chuyển |
| 3 | `ITOPhotoCamera.prefab` | — | Di chuyển |
| 4 | `Lantern.prefab` | — | Di chuyển |
| 5 | `LanternFuel.prefab` | — | Di chuyển |

### 5.78 `Assets/FpsHorrorKit/Prefabs/ModularParts` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModularParts` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Bodrum_Stairs.prefab` | — | Di chuyển |
| 2 | `Column.prefab` | — | Di chuyển |
| 3 | `Stairs.prefab` | — | Di chuyển |
| 4 | `StairsRailing.prefab` | — | Di chuyển |

### 5.79 `Assets/FpsHorrorKit/Prefabs/ModulerWalls` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModulerWalls` (10 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Wall_3x1.prefab` | — | Di chuyển |
| 2 | `Wall_3x1_DM.prefab` | — | Di chuyển |
| 3 | `Wall_3x1_DS.prefab` | — | Di chuyển |
| 4 | `Wall_3x1_DS_DM.prefab` | — | Di chuyển |
| 5 | `Wall_3x3.prefab` | — | Di chuyển |
| 6 | `Wall_3x3_DM.prefab` | — | Di chuyển |
| 7 | `Wall_3x3_DS.prefab` | — | Di chuyển |
| 8 | `Wall_3x3_DS_DM.prefab` | — | Di chuyển |
| 9 | `Wall_Window_2x3.prefab` | — | Di chuyển |
| 10 | `Wall_Window_3x3.prefab` | — | Di chuyển |

### 5.80 `Assets/FpsHorrorKit/Prefabs/Player` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Player` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Main Camera.prefab` | — | Di chuyển |
| 2 | `Player Interact.prefab` | — | Di chuyển |
| 3 | `Player.prefab` | — | Di chuyển |

### 5.81 `Assets/FpsHorrorKit/Prefabs/Props` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Props` (25 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `american_football.prefab` | — | Di chuyển |
| 2 | `brass_candleholder_a.prefab` | — | Di chuyển |
| 3 | `brass_candleholder_b.prefab` | — | Di chuyển |
| 4 | `brass_candleholder_c 1.prefab` | — | Di chuyển |
| 5 | `brass_goblet_a.prefab` | — | Di chuyển |
| 6 | `brass_goblet_b.prefab` | — | Di chuyển |
| 7 | `brass_goblet_c.prefab` | — | Di chuyển |
| 8 | `brass_pot_a.prefab` | — | Di chuyển |
| 9 | `brass_pot_b.prefab` | — | Di chuyển |
| 10 | `brass_vase_a.prefab` | — | Di chuyển |
| 11 | `brass_vase_b.prefab` | — | Di chuyển |
| 12 | `brass_vase_c.prefab` | — | Di chuyển |
| 13 | `cardboard_box.prefab` | — | Di chuyển |
| 14 | `Carved_Wooden_Elephant.prefab` | — | Di chuyển |
| 15 | `carved_wooden_plate.prefab` | — | Di chuyển |
| 16 | `fancy_picture_frame_a.prefab` | — | Di chuyển |
| 17 | `fancy_picture_frame_b.prefab` | — | Di chuyển |
| 18 | `hanging_picture_frame_c.prefab` | — | Di chuyển |
| 19 | `potted_plant.prefab` | — | Di chuyển |
| 20 | `vintage_pocket_watch.prefab` | — | Di chuyển |
| 21 | `wine_bottles_01_alsace.prefab` | — | Di chuyển |
| 22 | `wine_bottles_01_bordeaux.prefab` | — | Di chuyển |
| 23 | `wine_bottles_01_burgundy.prefab` | — | Di chuyển |
| 24 | `wine_bottles_01_champagne.prefab` | — | Di chuyển |
| 25 | `wooden_bowl_a.prefab` | — | Di chuyển |

### 5.82 `Assets/FpsHorrorKit/Prefabs/Systems` → `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Systems` (11 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Dialogue System.prefab` | — | Di chuyển |
| 2 | `Flashlight.prefab` | — | Di chuyển |
| 3 | `Hiding System.prefab` | — | Di chuyển |
| 4 | `Inspectable Paper.prefab` | — | Di chuyển |
| 5 | `Interact Camera Settings.prefab` | — | Di chuyển |
| 6 | `Interact Message System.prefab` | — | Di chuyển |
| 7 | `Inventory System.prefab` | — | Di chuyển |
| 8 | `Item Usage System.prefab` | — | Di chuyển |
| 9 | `Lantern.prefab` | — | Di chuyển |
| 10 | `Letter_UI_Manager.prefab` | — | Di chuyển |
| 11 | `Light Switch System.prefab` | — | Di chuyển |

### 5.83 `Assets/FpsHorrorKit/Scenes` → `Assets/_ThirdParty/FpsHorrorKit/Scenes` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DialogueSystemScene.unity` | — | Di chuyển |
| 2 | `DoorSystemsScene.unity` | — | Di chuyển |
| 3 | `Drawer&CabinetSystemScene.unity` | — | Di chuyển |
| 4 | `HidingSystemScene.unity` | — | Di chuyển |
| 5 | `InspectSystemScene.unity` | — | Di chuyển |
| 6 | `InventorySystemScene.unity` | — | Di chuyển |
| 7 | `Prototip.unity` | — | Di chuyển |
| 8 | `Quan.unity` | — | Di chuyển |
| 9 | `Tho.unity` | — | Di chuyển |

### 5.84 `Assets/FpsHorrorKit/Scenes/InventorySystemScene` → `Assets/_ThirdParty/FpsHorrorKit/Scenes/InventorySystemScene` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `MacDinh.asset` | — | Di chuyển |
| 2 | `Raining.asset` | — | Di chuyển |

### 5.85 `Assets/FpsHorrorKit/Scenes/MainScene` → `Assets/_ThirdParty/FpsHorrorKit/Scenes/MainScene` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Global Volume Profile.asset` | — | Di chuyển |
| 2 | `MainScene Baking Set.asset` | — | Di chuyển |
| 3 | `MainScene Baking Set.CellBricksData.bytes` | — | Di chuyển |
| 4 | `MainScene Baking Set.CellSharedData.bytes` | — | Di chuyển |
| 5 | `MainScene Baking Set.CellSupportData.bytes` | — | Di chuyển |
| 6 | `MainScene Baking Set-Default.CellData.bytes` | — | Di chuyển |
| 7 | `MainScene Baking Set-Default.CellOptionalData.bytes` | — | Di chuyển |
| 8 | `MainScene Baking Set-Default.CellProbeOcclusionData.bytes` | — | Di chuyển |
| 9 | `Sky and Fog Settings.asset` | — | Di chuyển |

### 5.86 `Assets/FpsHorrorKit/Scripts` → `Assets/_Project/Audio` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ElevenLabs_2026-10-30T14_41_57_Kristen - Natural, Upbeat and Focused_pvc_sp112_s50_sb80_v4.mp3` | — | Di chuyển |

### 5.87 `Assets/FpsHorrorKit/Scripts` → `Assets/_ThirdParty/FpsHorrorKit/Scripts` (17 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `AutoDoorTimer.cs` | — | Di chuyển |
| 2 | `CombinationSuitcase.cs` | — | Di chuyển |
| 3 | `DamageVignette0317.cs` | — | Di chuyển |
| 4 | `DoorMaconSequence.cs` | — | Di chuyển |
| 5 | `eyesScript.cs` | — | Di chuyển |
| 6 | `GhostMovement.cs` | — | Di chuyển |
| 7 | `HorrorLightFlicker.cs` | — | Di chuyển |
| 8 | `NhayMat.cs` | — | Di chuyển |
| 9 | `PictureFrameFallOnApproach.cs` | — | Di chuyển |
| 10 | `PlayerBlinkEffect.cs` | — | Di chuyển |
| 11 | `PlayerMovement.cs` | — | Di chuyển |
| 12 | `RockingChairSway.cs` | — | Di chuyển |
| 13 | `RoomEightMainMenu.cs` | — | Di chuyển |
| 14 | `SuitcaseCombinationUI.cs` | — | Di chuyển |
| 15 | `SuitcasePhotoViewer.cs` | — | Di chuyển |
| 16 | `TypewriterText.cs` | — | Di chuyển |
| 17 | `UIImageBlinkAndHide.cs` | — | Di chuyển |

### 5.88 `Assets/FpsHorrorKit/Scripts/DialogueSystems` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DialogueData.cs` | — | Di chuyển |
| 2 | `DialogueSystem.cs` | — | Di chuyển |

### 5.89 `Assets/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Npc_1_Dialogue1.asset` | — | Di chuyển |
| 2 | `Npc_1_Dialogue2.asset` | — | Di chuyển |
| 3 | `Npc_2_Dialogue1.asset` | — | Di chuyển |
| 4 | `Npc_2_Dialogue2.asset` | — | Di chuyển |
| 5 | `Npc_3_Dialogue_1.asset` | — | Di chuyển |

### 5.90 `Assets/FpsHorrorKit/Scripts/Editor` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/Editor` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ExorcismCircleCreator.cs` | — | Di chuyển |

### 5.91 `Assets/FpsHorrorKit/Scripts/FpsController` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/FpsController` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FpsAssets.inputactions` | — | Di chuyển |
| 2 | `FpsAssetsInputs.cs` | — | Di chuyển |
| 3 | `FpsController.cs` | — | Di chuyển |

### 5.92 `Assets/FpsHorrorKit/Scripts/InteractSystem` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `IInteractable.cs` | — | Di chuyển |
| 2 | `InteractCameraSettings.cs` | — | Di chuyển |
| 3 | `InteractMessageScript.cs` | — | Di chuyển |
| 4 | `PlayerInteract.cs` | — | Di chuyển |

### 5.93 `Assets/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` (8 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `ITOCamera.cs` | — | Di chuyển |
| 2 | `ITOCameraBattery.cs` | — | Di chuyển |
| 3 | `ITOInfoMessage.cs` | — | Di chuyển |
| 4 | `ITOKey.cs` | — | Di chuyển |
| 5 | `ITOLantern.cs` | — | Di chuyển |
| 6 | `ITOLanternFuel.cs` | — | Di chuyển |
| 7 | `ITOLightSwitch.cs` | — | Di chuyển |
| 8 | `ITONpc.cs` | — | Di chuyển |

### 5.94 `Assets/FpsHorrorKit/Scripts/InventorySyste` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Inventory.cs` | — | Di chuyển |
| 2 | `Item.cs` | — | Di chuyển |
| 3 | `ItemMethotReferances.cs` | — | Di chuyển |
| 4 | `Slot.prefab` | — | Di chuyển |
| 5 | `UIInventory.cs` | — | Di chuyển |
| 6 | `UISlot.cs` | — | Di chuyển |

### 5.95 `Assets/FpsHorrorKit/Scripts/InventorySyste/Items` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste/Items` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CameraBattery.asset` | — | Di chuyển |
| 2 | `Lantern.asset` | — | Di chuyển |
| 3 | `LanternFuel.asset` | — | Di chuyển |
| 4 | `PhotoCamera.asset` | — | Di chuyển |

### 5.96 `Assets/FpsHorrorKit/Scripts/PhotoCaptureSystem` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/PhotoCaptureSystem` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `PhotoAlbum.asset` | — | Di chuyển |
| 2 | `PhotoAlbum.cs` | — | Di chuyển |
| 3 | `PhotoCameraUpater.cs` | — | Di chuyển |
| 4 | `PhotoCaptureSystem.cs` | — | Di chuyển |
| 5 | `PhotoRenderTexture.renderTexture` | — | Di chuyển |

### 5.97 `Assets/FpsHorrorKit/Scripts/Systems` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/Systems` (25 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DoorSystem.cs` | — | Di chuyển |
| 2 | `DragToOpenSystem.cs` | — | Di chuyển |
| 3 | `DrawerSystem.cs` | — | Di chuyển |
| 4 | `FlashlightClueReveal.cs` | — | Di chuyển |
| 5 | `FlashLightUpdater.cs` | — | Di chuyển |
| 6 | `HealingPickup0317.cs` | — | Di chuyển |
| 7 | `HealthBar0317.cs` | — | Di chuyển |
| 8 | `HidingSystem.cs` | — | Di chuyển |
| 9 | `InspectSystem.cs` | — | Di chuyển |
| 10 | `ItemUsageSystem.cs` | — | Di chuyển |
| 11 | `LamAttackCutscene.cs` | — | Di chuyển |
| 12 | `LetterUIManager.cs` | — | Di chuyển |
| 13 | `Loop2ReturnTrigger.cs` | — | Di chuyển |
| 14 | `MainDoorOpeningSequence.cs` | — | Di chuyển |
| 15 | `MotherPhotoAltar.cs` | — | Di chuyển |
| 16 | `MotherPhotoHolder.cs` | — | Di chuyển |
| 17 | `MotherPhotoPickup.cs` | — | Di chuyển |
| 18 | `MusicBoxInteract.cs` | — | Di chuyển |
| 19 | `PlayerHealth0317.cs` | — | Di chuyển |
| 20 | `ProximityDoorOpener.cs` | — | Di chuyển |
| 21 | `RitualDoorSeal0317.cs` | — | Di chuyển |
| 22 | `RitualManager.cs` | — | Di chuyển |
| 23 | `RitualRoomDoorLock.cs` | — | Di chuyển |
| 24 | `RitualSealSymbol0317.cs` | — | Di chuyển |
| 25 | `SealInteract.cs` | — | Di chuyển |

### 5.98 `Assets/FpsHorrorKit/Scripts/TrapSystem` → `Assets/_ThirdParty/FpsHorrorKit/Scripts/TrapSystem` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `BearTrapJawVisual0317.cs` | — | Di chuyển |
| 2 | `DamageTrap0317.cs` | — | Di chuyển |
| 3 | `TrapDisarmInteract0317.cs` | — | Di chuyển |
| 4 | `TrapEscapeHandler0317.cs` | — | Di chuyển |
| 5 | `TrapEscapeZone0317.cs` | — | Di chuyển |
| 6 | `TrapGroup0317.cs` | — | Di chuyển |

### 5.99 `Assets/FpsHorrorKit/Shaders` → `Assets/_ThirdParty/FpsHorrorKit/Shaders` (7 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Floor.mat` | — | Di chuyển |
| 2 | `M_Plinth.mat` | — | Di chuyển |
| 3 | `M_Stairs.mat` | — | Di chuyển |
| 4 | `M_Wall.mat` | — | Di chuyển |
| 5 | `M_Wall_Bodrum.mat` | — | Di chuyển |
| 6 | `PatternedWall.shadergraph` | — | Di chuyển |
| 7 | `PatternedWallTriplanar.shadergraph` | — | Di chuyển |

### 5.100 `Assets/FpsHorrorKit/TextMesh Pro/Fonts` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Fonts` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `LiberationSans - OFL.txt` | — | Di chuyển |
| 2 | `LiberationSans.ttf` | — | Di chuyển |

### 5.101 `Assets/FpsHorrorKit/TextMesh Pro/Resources` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `LineBreaking Following Characters.txt` | — | Di chuyển |
| 2 | `LineBreaking Leading Characters.txt` | — | Di chuyển |
| 3 | `TMP Settings.asset` | — | Di chuyển |

### 5.102 `Assets/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `BRADHITC SDF.asset` | — | Di chuyển |
| 2 | `BRADHITC.TTF` | — | Di chuyển |
| 3 | `LiberationSans SDF - Drop Shadow.mat` | — | Di chuyển |
| 4 | `LiberationSans SDF - Fallback.asset` | — | Di chuyển |
| 5 | `LiberationSans SDF - Outline.mat` | — | Di chuyển |
| 6 | `LiberationSans SDF.asset` | — | Di chuyển |

### 5.103 `Assets/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `EmojiOne.asset` | — | Di chuyển |

### 5.104 `Assets/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Default Style Sheet.asset` | — | Di chuyển |

### 5.105 `Assets/FpsHorrorKit/TextMesh Pro/Shaders` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Shaders` (23 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `SDFFunctions.hlsl` | — | Di chuyển |
| 2 | `TMP_Bitmap.shader` | — | Di chuyển |
| 3 | `TMP_Bitmap-Custom-Atlas.shader` | — | Di chuyển |
| 4 | `TMP_Bitmap-Mobile.shader` | — | Di chuyển |
| 5 | `TMP_SDF Overlay.shader` | — | Di chuyển |
| 6 | `TMP_SDF SSD.shader` | — | Di chuyển |
| 7 | `TMP_SDF.shader` | — | Di chuyển |
| 8 | `TMP_SDF-HDRP LIT.shadergraph` | — | Di chuyển |
| 9 | `TMP_SDF-HDRP UNLIT.shadergraph` | — | Di chuyển |
| 10 | `TMP_SDF-Mobile Masking.shader` | — | Di chuyển |
| 11 | `TMP_SDF-Mobile Overlay.shader` | — | Di chuyển |
| 12 | `TMP_SDF-Mobile SSD.shader` | — | Di chuyển |
| 13 | `TMP_SDF-Mobile.shader` | — | Di chuyển |
| 14 | `TMP_SDF-Mobile-2-Pass.shader` | — | Di chuyển |
| 15 | `TMP_SDF-Surface.shader` | — | Di chuyển |
| 16 | `TMP_SDF-Surface-Mobile.shader` | — | Di chuyển |
| 17 | `TMP_SDF-URP Lit.shadergraph` | — | Di chuyển |
| 18 | `TMP_SDF-URP Unlit.shadergraph` | — | Di chuyển |
| 19 | `TMP_Sprite.shader` | — | Di chuyển |
| 20 | `TMPro.cginc` | — | Di chuyển |
| 21 | `TMPro_Mobile.cginc` | — | Di chuyển |
| 22 | `TMPro_Properties.cginc` | — | Di chuyển |
| 23 | `TMPro_Surface.cginc` | — | Di chuyển |

### 5.106 `Assets/FpsHorrorKit/TextMesh Pro/Sprites` → `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Sprites` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `EmojiOne Attribution.txt` | — | Di chuyển |
| 2 | `EmojiOne.json` | — | Di chuyển |
| 3 | `EmojiOne.png` | — | Di chuyển |

### 5.107 `Assets/FpsHorrorKit/Textures` → `Assets/_ThirdParty/FpsHorrorKit/Textures` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `distressed_painted_planks_diff_2k.png` | — | Di chuyển |
| 2 | `distressed_painted_planks_nor_gl_2k.exr` | — | Di chuyển |
| 3 | `wood_floor_deck_diff_2k.png` | — | Di chuyển |
| 4 | `wood_floor_deck_nor_gl_2k.png` | — | Di chuyển |

### 5.108 `Assets/FpsHorrorKit/Textures/Decal` → `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal` (10 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Ay.png` | — | Di chuyển |
| 2 | `Blood_1.png` | — | Di chuyển |
| 3 | `CandleFlame.png` | — | Di chuyển |
| 4 | `DM_Blood_a.mat` | — | Di chuyển |
| 5 | `Fingerprint.png` | — | Di chuyển |
| 6 | `FootPrint_01.png` | — | Di chuyển |
| 7 | `HandPrint.png` | — | Di chuyển |
| 8 | `Kirik.png` | — | Di chuyển |
| 9 | `ShoePrint_01_L.png` | — | Di chuyển |
| 10 | `ShoePrint_01_R.png` | — | Di chuyển |

### 5.109 `Assets/FpsHorrorKit/Textures/Decal/Texts` → `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal/Texts` (17 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CanYouHereThem.png` | — | Di chuyển |
| 2 | `DontLookBehindYou.png` | — | Di chuyển |
| 3 | `HelpMe.png` | — | Di chuyển |
| 4 | `ImStillHere.png` | — | Di chuyển |
| 5 | `IseeYou.png` | — | Di chuyển |
| 6 | `ItHurts.png` | — | Di chuyển |
| 7 | `ItsTooLate.png` | — | Di chuyển |
| 8 | `LeaveNow.png` | — | Di chuyển |
| 9 | `NoHopeLeft.png` | — | Di chuyển |
| 10 | `NotAlone.png` | — | Di chuyển |
| 11 | `Run.png` | — | Di chuyển |
| 12 | `TheyAreWatvhing.png` | — | Di chuyển |
| 13 | `TurnBack.png` | — | Di chuyển |
| 14 | `WhereIsMyBody.png` | — | Di chuyển |
| 15 | `WhosThere.png` | — | Di chuyển |
| 16 | `WhyDidComeHere.png` | — | Di chuyển |
| 17 | `YouWillNeverEscape.png` | — | Di chuyển |

### 5.110 `Assets/FpsHorrorKit/Textures/Patterns` → `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DuvarKagidi_Desen_White.png` | — | Di chuyển |
| 2 | `DuvarKagidi_Fantasy.png` | — | Di chuyển |
| 3 | `DuvarKagidi_Horizontal.png` | — | Di chuyển |
| 4 | `DuvarKagidi_Horizontal_2.png` | — | Di chuyển |
| 5 | `DuvarKagidi_KareDesen.png` | — | Di chuyển |
| 6 | `DuvarKagidi_Randomize 1.png` | — | Di chuyển |
| 7 | `DuvarKagidi_Randomize.png` | — | Di chuyển |
| 8 | `DuvarKagidi_VerticalDesen.png` | — | Di chuyển |
| 9 | `Kir.png` | — | Di chuyển |

### 5.111 `Assets/FpsHorrorKit/Textures/Patterns/Colorful` → `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/Colorful` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Wallpaper_Dark.png` | — | Di chuyển |
| 2 | `Wallpaper_Light.png` | — | Di chuyển |

### 5.112 `Assets/FpsHorrorKit/Textures/Patterns/FavoritePatterns` → `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/FavoritePatterns` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DuvarKagidi_KalpDesen.png` | — | Di chuyển |
| 2 | `DuvarKagidi_NoktaDesen.png` | — | Di chuyển |
| 3 | `DuvarKagidi_Sicratma.png` | — | Di chuyển |
| 4 | `DuvarKagidi_ThickKir.png` | — | Di chuyển |
| 5 | `DuvarKagidi_ThinKir.png` | — | Di chuyển |

### 5.113 `Assets/Free-FlashLight/Materials` → `Assets/_ThirdParty/Free-FlashLight/Materials` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FlashLight.mat` | — | Di chuyển |
| 2 | `Lense.mat` | — | Di chuyển |
| 3 | `Reflector.mat` | — | Di chuyển |
| 4 | `Rubber.mat` | — | Di chuyển |

### 5.114 `Assets/Free-FlashLight/Models` → `Assets/_ThirdParty/Free-FlashLight/Models` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FlashLight.fbx` | — | Di chuyển |

### 5.115 `Assets/Free-FlashLight/Prefabs` → `Assets/_ThirdParty/Free-FlashLight/Prefabs` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FlashLight.prefab` | — | Di chuyển |

### 5.116 `Assets/Free-FlashLight/Scenes` → `Assets/_ThirdParty/Free-FlashLight/Scenes` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FlashLightSample.unity` | — | Di chuyển |

### 5.117 `Assets/Free-FlashLight/Textures` → `Assets/_ThirdParty/Free-FlashLight/Textures` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Albedo.png` | — | Di chuyển |
| 2 | `Roughness.png` | — | Di chuyển |

### 5.118 `Assets/Ghost` → `Assets/_Project/Prefabs/Model` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `DOE Ghost.fbx` | `Ghost.fbx` | Đổi tên |

### 5.119 `Assets/Ghost` → `Assets/_Project/Materials/Ghost` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Body.mat` | — | Di chuyển |
| 2 | `Dress.mat` | — | Di chuyển |
| 3 | `Hair.mat` | — | Di chuyển |

### 5.120 `Assets/Ghost` → `Assets/_Project/Image/Ghost` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `lambert1_Base_Color.png` | — | Di chuyển |
| 2 | `lambert1_Metallic.png` | — | Di chuyển |
| 3 | `lambert1_Mixed_AO.png` | — | Di chuyển |
| 4 | `lambert1_Normal_OpenGL.png` | — | Di chuyển |
| 5 | `lambert1_Roughness.png` | — | Di chuyển |

### 5.121 `Assets/Ghost` → `Assets/_Project/Scripts/Ghost` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `GhostController.controller` | — | Di chuyển |

### 5.122 `Assets/HopSo` → `Assets/_Project/Prefabs/Model` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Explode 2.fbx` | `Skull.fbx` | Đổi tên |

### 5.123 `Assets/HopSo` → `Assets/_Project/Materials/Skull` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `New Material.mat` | `Skull.mat` | Đổi tên |

### 5.124 `Assets/Image/Art` → `Assets/_Project/Image/Texture/Photo` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `2mecon.png` | — | Di chuyển |
| 2 | `46ee10da-4e7a-4095-bfdb-737aa08e218a.png` | — | Di chuyển |
| 3 | `anhme.png` | — | Di chuyển |

### 5.125 `Assets/Image/Art` → `Assets/_Project/Image/Texture/Letter` (7 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `2.png` | — | Di chuyển |
| 2 | `3.png` | — | Di chuyển |
| 3 | `4.png` | — | Di chuyển |
| 4 | `5.png` | — | Di chuyển |
| 5 | `6.png` | — | Di chuyển |
| 6 | `lathu1.png` | — | Di chuyển |
| 7 | `thu7.png` | — | Di chuyển |

### 5.126 `Assets/Image/PIcture` → `Assets/_Project/Image/PIcture` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `1792416441684_2294850091621530808_411634269032532182_b9599c967a0652b5c27427823a4356cf 1.jpg` | — | Di chuyển |
| 2 | `1792416441684_2294850091621530808_411634269032532182_b9599c967a0652b5c27427823a4356cf.jpg` | — | Di chuyển |
| 3 | `AnhHinhNhan.png` | — | Di chuyển |
| 4 | `AnhPhongTho.png` | — | Di chuyển |
| 5 | `nha-ma-da-lat-6_1698753546.png` | — | Di chuyển |

### 5.127 `Assets/Image/Texture` → `Assets/_Project/Image/Texture/Graffiti` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `9447cb8a-45c5-4c85-b8e0-b321d3106b69-fotor-bg-remover-20261019204518.png` | `Graffiti_1.png` | Đổi tên |
| 2 | `exec-9b871a8b-9def-426e-8edd-e51c3973ddad.png` | `Graffiti_2.png` | Đổi tên |
| 3 | `goi y.png` | `Graffiti_3.png` | Đổi tên |
| 4 | `image-fotor-bg-remover-2026101920043.png` | `Graffiti_4.png` | Đổi tên |

### 5.128 `Assets/Image/Texture` → `Assets/_Project/Image/Texture/Fulu` (3 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `bua.png` | `Fulu_Wall.png` | Đổi tên |
| 2 | `buato.png` | `Fulu_Single_1.png` | Đổi tên |
| 3 | `buato1.png` | `Fulu_Single_2.png` | Đổi tên |

### 5.129 `Assets/Image/Texture` → `Assets/_Project/Image/Texture/Blood` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `mau.png` | `Blood_1.png` | Đổi tên |
| 2 | `mau1.png` | `Blood_2.png` | Đổi tên |

### 5.130 `Assets/Image/Texture` → `Assets/_Project/Image/Texture` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `5ebeda43-16f2-4723-8068-e6fda9a9efe9.png` | `Bagua.png` | Đổi tên |
| 2 | `vong.png` | `Spell_Texture_Rice.png` | Đổi tên |

### 5.131 `Assets/LongTieng` → `Assets/_Project/Audio` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `“...Lạnh quá.”-21625022ba_5022ba.mp3` | — | Di chuyển |
| 2 | `Sao cửa lại bị -9f859c87f6_9c87f6.mp3` | — | Di chuyển |

### 5.132 `Assets/MACon` → `Assets/_Project/MACon` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Catwalk Walk.fbx` | — | Di chuyển |
| 2 | `DOE Ghost.controller` | — | Di chuyển |
| 3 | `GirlScout_diffuse.png` | — | Di chuyển |
| 4 | `GirlScout_diffuse_blackhair.png` | — | Di chuyển |
| 5 | `GirlScout_eye.png` | — | Di chuyển |
| 6 | `GirlScout_NoBackStick.asset` | — | Di chuyển |
| 7 | `GirlScout_normal.png` | — | Di chuyển |
| 8 | `GirlScout_specular.png` | — | Di chuyển |
| 9 | `Macon.controller` | — | Di chuyển |

### 5.133 `Assets/MACon/Materials` → `Assets/_Project/MACon/Materials` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `GirlScout_diffuse 1.mat` | — | Di chuyển |
| 2 | `GirlScout_diffuse 2.mat` | — | Di chuyển |
| 3 | `GirlScout_diffuse.mat` | — | Di chuyển |
| 4 | `GirlScout_diffuse_blackhair.mat` | — | Di chuyển |
| 5 | `GirlScout_eye.mat` | — | Di chuyển |
| 6 | `GirlScout_specular.mat` | — | Di chuyển |

### 5.134 `Assets/MaConBay` → `Assets/_Project/MaConBay` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `1.fbx` | — | Di chuyển |
| 2 | `555.jpg` | — | Di chuyển |
| 3 | `GhostCanvasJumpscare.cs` | — | Di chuyển |
| 4 | `MaHu.png` | — | Di chuyển |

### 5.135 `Assets/MaConBay/Materials` → `Assets/_Project/MaConBay/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `555.mat` | — | Di chuyển |

### 5.136 `Assets/MaConBay/music box` → `Assets/_Project/MaConBay/music box` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Box_anim_new_2.fbx` | — | Di chuyển |
| 2 | `lambert1_Base_Color.png` | — | Di chuyển |
| 3 | `lambert1_Metallic.png` | — | Di chuyển |
| 4 | `lambert1_Normal_OpenGL.png` | — | Di chuyển |
| 5 | `lambert1_Roughness.png` | — | Di chuyển |
| 6 | `MusicBox.controller` | — | Di chuyển |

### 5.137 `Assets/MaConBay/music box/Materials` → `Assets/_Project/MaConBay/music box/Materials` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `lambert1_Base_Color.mat` | — | Di chuyển |
| 2 | `lambert1_Metallic.mat` | — | Di chuyển |

### 5.138 `Assets/MAMe 1` → `Assets/_Project/MAMe 1` (15 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Button Pushing.fbx` | — | Di chuyển |
| 2 | `GhostFloorTrigger.cs` | — | Di chuyển |
| 3 | `Hanging Idle.fbx` | — | Di chuyển |
| 4 | `MaDiQuaTrai.cs` | — | Di chuyển |
| 5 | `Mame.controller` | — | Di chuyển |
| 6 | `MotherGhostAutoDoor.cs` | — | Di chuyển |
| 7 | `Opening.fbx` | — | Di chuyển |
| 8 | `StairsRailingGhostTrigger.cs` | — | Di chuyển |
| 9 | `Walk Strafe Left.fbx` | — | Di chuyển |
| 10 | `Walking.fbx` | — | Di chuyển |
| 11 | `XuatHienTreoNguoc.cs` | — | Di chuyển |
| 12 | `zombie_body_diffuse.png` | — | Di chuyển |
| 13 | `zombie_diffuse.png` | — | Di chuyển |
| 14 | `zombie_normal.png` | — | Di chuyển |
| 15 | `zombie_specular.png` | — | Di chuyển |

### 5.139 `Assets/MAMe 1/Materials` → `Assets/_Project/MAMe 1/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `zombie_body_diffuse.mat` | — | Di chuyển |

### 5.140 `Assets/Mame/source/Womann` → `Assets/_Project/Mame/source/Womann` (14 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Womann.mtl` | — | Di chuyển |
| 2 | `Womann.obj` | — | Di chuyển |
| 3 | `Womann_Body_diffuse.png` | — | Di chuyển |
| 4 | `Womann_Body_gloss.png` | — | Di chuyển |
| 5 | `Womann_Body_normal.png` | — | Di chuyển |
| 6 | `Womann_Body_specular.png` | — | Di chuyển |
| 7 | `Womann_Hair_diffuse.png` | — | Di chuyển |
| 8 | `Womann_Hair_gloss.png` | — | Di chuyển |
| 9 | `Womann_Hair_normal.png` | — | Di chuyển |
| 10 | `Womann_Hair_specular.png` | — | Di chuyển |
| 11 | `Womann_Top_diffuse.png` | — | Di chuyển |
| 12 | `Womann_Top_gloss.png` | — | Di chuyển |
| 13 | `Womann_Top_normal.png` | — | Di chuyển |
| 14 | `Womann_Top_specular.png` | — | Di chuyển |

### 5.141 `Assets/Mame/textures` → `Assets/_Project/Mame/textures` (12 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Womann_Body_diffuse.png` | — | Di chuyển |
| 2 | `Womann_Body_gloss.png` | — | Di chuyển |
| 3 | `Womann_Body_normal.png` | — | Di chuyển |
| 4 | `Womann_Body_specular.png` | — | Di chuyển |
| 5 | `Womann_Hair_diffuse.png` | — | Di chuyển |
| 6 | `Womann_Hair_gloss.png` | — | Di chuyển |
| 7 | `Womann_Hair_normal.png` | — | Di chuyển |
| 8 | `Womann_Hair_specular.png` | — | Di chuyển |
| 9 | `Womann_Top_diffuse.png` | — | Di chuyển |
| 10 | `Womann_Top_gloss.png` | — | Di chuyển |
| 11 | `Womann_Top_normal.png` | — | Di chuyển |
| 12 | `Womann_Top_specular.png` | — | Di chuyển |

### 5.142 `Assets/Materials` → `Assets/_Project/Materials` (13 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `1792416441684_2294850091621530808_411634269032532182_b9599c967a0652b5c27427823a4356cf.mat` | — | Di chuyển |
| 2 | `1793013499426_782906075553311933_g760111020785204716_43f0462164889d39d96af0c1df0a1282.mat` | — | Di chuyển |
| 3 | `46ee10da-4e7a-4095-bfdb-737aa08e218a.mat` | — | Di chuyển |
| 4 | `Diary_01.mat` | — | Di chuyển |
| 5 | `image-fotor-bg-remover-2026101920043.mat` | — | Di chuyển |
| 6 | `mau.mat` | — | Di chuyển |
| 7 | `mau1.mat` | — | Di chuyển |
| 8 | `New Material 1.mat` | — | Di chuyển |
| 9 | `New Material 2.mat` | — | Di chuyển |
| 10 | `New Material 3.mat` | — | Di chuyển |
| 11 | `New Material 4.mat` | — | Di chuyển |
| 12 | `New Material.mat` | — | Di chuyển |
| 13 | `vong.mat` | — | Di chuyển |

### 5.143 `Assets/Nen` → `Assets/_Project/Nen` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CandleStick.fbx` | — | Di chuyển |
| 2 | `CandleStick_LP_Albedo.tga.png` | — | Di chuyển |
| 3 | `CandleStick_Lp_AO.tga.png` | — | Di chuyển |
| 4 | `CandleStick_Lp_Metalness.tga.png` | — | Di chuyển |
| 5 | `CandleStick_Lp_Normals.tga.png` | — | Di chuyển |
| 6 | `CandleStick_Lp_Roughness.tga.png` | — | Di chuyển |

### 5.144 `Assets/Nen/Materials` → `Assets/_Project/Nen/Materials` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `CandleStick_LP_Albedo.tga 1.mat` | — | Di chuyển |
| 2 | `CandleStick_LP_Albedo.tga 2.mat` | — | Di chuyển |
| 3 | `CandleStick_LP_Albedo.tga 3.mat` | — | Di chuyển |
| 4 | `CandleStick_LP_Albedo.tga 4.mat` | — | Di chuyển |
| 5 | `CandleStick_LP_Albedo.tga.mat` | — | Di chuyển |
| 6 | `CandleStick_Lp_AO.tga 1.mat` | — | Di chuyển |
| 7 | `CandleStick_Lp_AO.tga.mat` | — | Di chuyển |
| 8 | `CandleStick_Lp_Normals.tga.mat` | — | Di chuyển |
| 9 | `CandleStick_Lp_Roughness.tga.mat` | — | Di chuyển |

### 5.145 `Assets/OngLam` → `Assets/_Project/Image/OngLam` (5 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Ch31_1001_Diffuse.png` | — | Di chuyển |
| 2 | `Ch31_1001_Glossiness.png` | — | Di chuyển |
| 3 | `Ch31_1001_Normal.png` | — | Di chuyển |
| 4 | `Ch31_1001_Specular.png` | — | Di chuyển |
| 5 | `Ch31_1002_Diffuse.png` | — | Di chuyển |

### 5.146 `Assets/OngLam` → `Assets/_Project/Prefabs/Model/OngLam` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Idle.fbx` | — | Di chuyển |
| 2 | `Male Sitting Pose.fbx` | — | Di chuyển |

### 5.147 `Assets/OngLam` → `Assets/_Project/OngLam` (6 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `OngLam.controller` | — | Di chuyển |
| 2 | `OngLamMoCua.controller` | — | Di chuyển |
| 3 | `OngLamNgoi.controller` | — | Di chuyển |
| 4 | `run.fbx` | — | Di chuyển |
| 5 | `Slow Run.fbx` | — | Di chuyển |
| 6 | `Standing Melee Attack Downward.fbx` | — | Di chuyển |

### 5.148 `Assets/OngLam/Materials` → `Assets/_Project/Materials/OngLam` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Ch31_1001_Diffuse.mat` | — | Di chuyển |
| 2 | `Ch31_1001_Glossiness.mat` | — | Di chuyển |

### 5.149 `Assets/Player` → `Assets/_Project/Prefabs/Model` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `male3dscan.fbx` | `Player.fbx` | Đổi tên |

### 5.150 `Assets/Player` → `Assets/_Project/Image/Texture/Player` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `body.jpeg` | `Player_Texture_Body.jpeg` | Đổi tên |

### 5.151 `Assets/Player/Materials` → `Assets/_Project/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `body.mat` | `Player_Body.mat` | Đổi tên |

### 5.152 `Assets/Quantai` → `Assets/_Project/Quantai` (15 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `Chipboard001_2K_Color.png` | — | Di chuyển |
| 2 | `Chipboard001_2K_NormalDX.png` | — | Di chuyển |
| 3 | `Chipboard001_2K_Roughness.png` | — | Di chuyển |
| 4 | `Fabric060_2K_AmbientOcclusion.png` | — | Di chuyển |
| 5 | `Fabric060_2K_Color.png` | — | Di chuyển |
| 6 | `Fabric060_2K_NormalDX.png` | — | Di chuyển |
| 7 | `Fabric060_2K_Roughness.png` | — | Di chuyển |
| 8 | `ItalianCoffin.fbx` | — | Di chuyển |
| 9 | `Metal034_2K_Color.png` | — | Di chuyển |
| 10 | `Metal034_2K_Metalness.png` | — | Di chuyển |
| 11 | `Metal034_2K_NormalDX.png` | — | Di chuyển |
| 12 | `Metal034_2K_Roughness.png` | — | Di chuyển |
| 13 | `Wood067_2K_Color.png` | — | Di chuyển |
| 14 | `Wood067_2K_NormalDX.png` | — | Di chuyển |
| 15 | `Wood067_2K_Roughness.png` | — | Di chuyển |

### 5.153 `Assets/Rain/Prefab` → `Assets/_Project/Rain/Prefab` (2 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `MuaNho.prefab` | — | Di chuyển |
| 2 | `MuaTo.prefab` | — | Di chuyển |

### 5.154 `Assets/RoomAudio` → `Assets/_Project/RoomAudio` (4 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `BGM03.ogg` | — | Di chuyển |
| 2 | `nhacmusicbox.mp3` | — | Di chuyển |
| 3 | `nhacnen.mp3` | — | Di chuyển |
| 4 | `niem-nam-mo-a-di-da-phat-thich-tri-thoat-2_4l1UBdi0 (1).mp3` | — | Di chuyển |

### 5.155 `Assets/Sky/Cubemaps/Classic` → `Assets/_ThirdParty/Sky/Cubemaps/Classic` (36 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FS000_Day_01.mat` | — | Di chuyển |
| 2 | `FS000_Day_01.png` | — | Di chuyển |
| 3 | `FS000_Day_01_Sunless.mat` | — | Di chuyển |
| 4 | `FS000_Day_01_Sunless.png` | — | Di chuyển |
| 5 | `FS000_Day_02.mat` | — | Di chuyển |
| 6 | `FS000_Day_02.png` | — | Di chuyển |
| 7 | `FS000_Day_02_Sunless.mat` | — | Di chuyển |
| 8 | `FS000_Day_02_Sunless.png` | — | Di chuyển |
| 9 | `FS000_Day_03.mat` | — | Di chuyển |
| 10 | `FS000_Day_03.png` | — | Di chuyển |
| 11 | `FS000_Day_03_Sunless.mat` | — | Di chuyển |
| 12 | `FS000_Day_03_Sunless.png` | — | Di chuyển |
| 13 | `FS000_Day_04.mat` | — | Di chuyển |
| 14 | `FS000_Day_04.png` | — | Di chuyển |
| 15 | `FS000_Day_04_Sunless.mat` | — | Di chuyển |
| 16 | `FS000_Day_04_Sunless.png` | — | Di chuyển |
| 17 | `FS000_Day_05.mat` | — | Di chuyển |
| 18 | `FS000_Day_05.png` | — | Di chuyển |
| 19 | `FS000_Day_05_Sunless.mat` | — | Di chuyển |
| 20 | `FS000_Day_05_Sunless.png` | — | Di chuyển |
| 21 | `FS000_Day_06.mat` | — | Di chuyển |
| 22 | `FS000_Day_06.png` | — | Di chuyển |
| 23 | `FS000_Day_06_Sunless.mat` | — | Di chuyển |
| 24 | `FS000_Day_06_Sunless.png` | — | Di chuyển |
| 25 | `FS000_Night_01.mat` | — | Di chuyển |
| 26 | `FS000_Night_01.png` | — | Di chuyển |
| 27 | `FS000_Night_01_Moonless.mat` | — | Di chuyển |
| 28 | `FS000_Night_01_Moonless.png` | — | Di chuyển |
| 29 | `FS000_Night_02.mat` | — | Di chuyển |
| 30 | `FS000_Night_02.png` | — | Di chuyển |
| 31 | `FS000_Night_02_Moonless.mat` | — | Di chuyển |
| 32 | `FS000_Night_02_Moonless.png` | — | Di chuyển |
| 33 | `FS000_Night_03.mat` | — | Di chuyển |
| 34 | `FS000_Night_03.png` | — | Di chuyển |
| 35 | `FS000_Night_03_Moonless.mat` | — | Di chuyển |
| 36 | `FS000_Night_03_Moonless.png` | — | Di chuyển |

### 5.156 `Assets/Sky/Panoramics/FS002` → `Assets/_ThirdParty/Sky/Panoramics/FS002` (16 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FS002_Day.mat` | — | Di chuyển |
| 2 | `FS002_Day.png` | — | Di chuyển |
| 3 | `FS002_Day_Sunless.mat` | — | Di chuyển |
| 4 | `FS002_Day_Sunless.png` | — | Di chuyển |
| 5 | `FS002_Night.mat` | — | Di chuyển |
| 6 | `FS002_Night.png` | — | Di chuyển |
| 7 | `FS002_Night_Moonless.mat` | — | Di chuyển |
| 8 | `FS002_Night_Moonless.png` | — | Di chuyển |
| 9 | `FS002_Rainy.mat` | — | Di chuyển |
| 10 | `FS002_Rainy.png` | — | Di chuyển |
| 11 | `FS002_Snowy.mat` | — | Di chuyển |
| 12 | `FS002_Snowy.png` | — | Di chuyển |
| 13 | `FS002_Sunrise.mat` | — | Di chuyển |
| 14 | `FS002_Sunrise.png` | — | Di chuyển |
| 15 | `FS002_Sunset.mat` | — | Di chuyển |
| 16 | `FS002_Sunset.png` | — | Di chuyển |

### 5.157 `Assets/Sky/Panoramics/FS003` → `Assets/_ThirdParty/Sky/Panoramics/FS003` (16 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FS003_Day.mat` | — | Di chuyển |
| 2 | `FS003_Day.png` | — | Di chuyển |
| 3 | `FS003_Day_Sunless.mat` | — | Di chuyển |
| 4 | `FS003_Day_Sunless.png` | — | Di chuyển |
| 5 | `FS003_Night.mat` | — | Di chuyển |
| 6 | `FS003_Night.png` | — | Di chuyển |
| 7 | `FS003_Night_Moonless.mat` | — | Di chuyển |
| 8 | `FS003_Night_Moonless.png` | — | Di chuyển |
| 9 | `FS003_Rainy.mat` | — | Di chuyển |
| 10 | `FS003_Rainy.png` | — | Di chuyển |
| 11 | `FS003_Snowy.mat` | — | Di chuyển |
| 12 | `FS003_Snowy.png` | — | Di chuyển |
| 13 | `FS003_Sunrise.mat` | — | Di chuyển |
| 14 | `FS003_Sunrise.png` | — | Di chuyển |
| 15 | `FS003_Sunset.mat` | — | Di chuyển |
| 16 | `FS003_Sunset.png` | — | Di chuyển |

### 5.158 `Assets/Sky/Panoramics/FS013` → `Assets/_ThirdParty/Sky/Panoramics/FS013` (17 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FS013_Day.mat` | — | Di chuyển |
| 2 | `FS013_Day.png` | — | Di chuyển |
| 3 | `FS013_Day_Sunless.mat` | — | Di chuyển |
| 4 | `FS013_Day_Sunless.png` | — | Di chuyển |
| 5 | `FS013_Night.mat` | — | Di chuyển |
| 6 | `FS013_Night.png` | — | Di chuyển |
| 7 | `FS013_Night_Moonless.mat` | — | Di chuyển |
| 8 | `FS013_Night_Moonless.png` | — | Di chuyển |
| 9 | `FS013_Rainy 1.png` | — | Di chuyển |
| 10 | `FS013_Rainy.mat` | — | Di chuyển |
| 11 | `FS013_Rainy.png` | — | Di chuyển |
| 12 | `FS013_Snowy.mat` | — | Di chuyển |
| 13 | `FS013_Snowy.png` | — | Di chuyển |
| 14 | `FS013_Sunrise.mat` | — | Di chuyển |
| 15 | `FS013_Sunrise.png` | — | Di chuyển |
| 16 | `FS013_Sunset.mat` | — | Di chuyển |
| 17 | `FS013_Sunset.png` | — | Di chuyển |

### 5.159 `Assets/Sky/Panoramics/FS017` → `Assets/_ThirdParty/Sky/Panoramics/FS017` (17 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FS017_Day.mat` | — | Di chuyển |
| 2 | `FS017_Day.png` | — | Di chuyển |
| 3 | `FS017_Day_Sunless.mat` | — | Di chuyển |
| 4 | `FS017_Day_Sunless.png` | — | Di chuyển |
| 5 | `FS017_Night.mat` | — | Di chuyển |
| 6 | `FS017_Night.png` | — | Di chuyển |
| 7 | `FS017_Night_Moonless.mat` | — | Di chuyển |
| 8 | `FS017_Night_Moonless.png` | — | Di chuyển |
| 9 | `FS017_Rainy 1.png` | — | Di chuyển |
| 10 | `FS017_Rainy.mat` | — | Di chuyển |
| 11 | `FS017_Rainy.png` | — | Di chuyển |
| 12 | `FS017_Snowy.mat` | — | Di chuyển |
| 13 | `FS017_Snowy.png` | — | Di chuyển |
| 14 | `FS017_Sunrise.mat` | — | Di chuyển |
| 15 | `FS017_Sunrise.png` | — | Di chuyển |
| 16 | `FS017_Sunset.mat` | — | Di chuyển |
| 17 | `FS017_Sunset.png` | — | Di chuyển |

### 5.160 `Assets/Studio` → `Assets/_Project/Studio` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `tieng mua.mp3` | — | Di chuyển |

### 5.161 `Assets/Tree` → `Assets/_ThirdParty/Tree` (9 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `bca9eaed0a17233bdf19af2dec67c1e5.jpeg` | `TrreeBark_Texture.jpeg` | Đổi tên |
| 2 | `Material.003.mat` | — | Di chuyển |
| 3 | `Material.004.mat` | — | Di chuyển |
| 4 | `Material.mat` | — | Di chuyển |
| 5 | `NormalMap (2).png` | — | Di chuyển |
| 6 | `NormalMap.png` | — | Di chuyển |
| 7 | `Без названия113_20260812112021.png` | — | Di chuyển |
| 8 | `загруженное.png` | — | Di chuyển |
| 9 | `лес3 crtn vjlba.fbx` | — | Di chuyển |

### 5.162 `Assets/Tuong` → `Assets/_Project/Tuong` (11 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FoKan230827.jpeg` | — | Di chuyển |
| 2 | `FoKan230827.jpg` | — | Di chuyển |
| 3 | `FoKan230827.mtl` | — | Di chuyển |
| 4 | `FoKan230827.obj` | — | Di chuyển |
| 5 | `FoKan230827-ao.jpg` | — | Di chuyển |
| 6 | `FoKan230827-metallic.jpeg` | — | Di chuyển |
| 7 | `FoKan230827-metallic.jpg` | — | Di chuyển |
| 8 | `FoKan230827-normal.jpeg` | — | Di chuyển |
| 9 | `FoKan230827-normal.jpg` | — | Di chuyển |
| 10 | `FoKan230827-roughness.jpeg` | — | Di chuyển |
| 11 | `FoKan230827-roughness.jpg` | — | Di chuyển |

### 5.163 `Assets/Tuong/Materials` → `Assets/_Project/Tuong/Materials` (1 file)

| # | File cũ | File mới | Loại |
| ---: | --- | --- | --- |
| 1 | `FoKan230827.mat` | — | Di chuyển |

## 6. Thư mục mới được tạo (`.meta` mới)

15 file `.meta` dưới đây là của **thư mục mới**, không phải asset. Chúng không có file nội dung đi kèm.

| # | Đường dẫn `.meta` | GUID |
| ---: | --- | --- |
| 1 | `Assets/_Project/Image/Texture/Fulu.meta` | `9ba02a41589da5e478ed7b02df41cd34` |
| 2 | `Assets/_Project/Image/Texture/Graffiti.meta` | `82cb3ad115077a34a9ab683e247cc05f` |
| 3 | `Assets/_Project/Image/Texture/Letter.meta` | `eedbf3e1a07c1d6469270b1721e47f9e` |
| 4 | `Assets/_Project/Image/Texture/Photo.meta` | `bb6ce6a21263e2c498d4735996841c05` |
| 5 | `Assets/_Project/Image/Texture/Player.meta` | `e356653971ecb694f90af1fd472beaf3` |
| 6 | `Assets/_Project/Materials/Dau_ThienLinh.meta` | `569d2f2c50359ec4b9895615a858bd05` |
| 7 | `Assets/_Project/Materials/DiaryImages.meta` | `82dc4ba38dba52a4bbd8c2688279db27` |
| 8 | `Assets/_Project/Materials/Ghost.meta` | `a7f18c26e9296f84d80e22ab9ae1064c` |
| 9 | `Assets/_Project/Materials/OngLam.meta` | `e909a7bab2311a84a83cb78f368cdddc` |
| 10 | `Assets/_Project/Materials/Skull.meta` | `24ca740daafe397489ca47f2a9442694` |
| 11 | `Assets/_Project/Prefabs/Model.meta` | `30bcc7c687e340d4e94c975aa9320d0c` |
| 12 | `Assets/_Project/Prefabs/Model/OngLam.meta` | `5ff65ad1ea925f843a3a6a2201e483aa` |
| 13 | `Assets/_Project/Scripts/Ghost.meta` | `3f82eb19cd1f6594fbf8e2d63db925f4` |
| 14 | `Assets/_ThirdParty.meta` | `029afb3ac3e1b3a43ab378aa4f0e6664` |
| 15 | `Assets/_Unknown.meta` | `3ccbdbd76df09d04b8fc3130c3fb2ec8` |

## 7. `.meta` bị xoá không có thay thế

Đây là các `.meta` của thư mục cũ đã bị xoá hẳn (thư mục không còn tồn tại ở vị trí mới nào).

| # | Đường dẫn `.meta` đã xoá |
| ---: | --- |
| 1 | `Assets/Ghost/Textures.meta` |
| 2 | `Assets/HopSo.meta` |
| 3 | `Assets/LongTieng.meta` |
| 4 | `Assets/OngLam/Materials.meta` |
| 5 | `Assets/_0317.meta` |
| 6 | `Assets/_0317/Materials.meta` |
| 7 | `Assets/Image/Art.meta` |
| 8 | `Assets/Image/Background.meta` |
| 9 | `Assets/Image/Misc.meta` |
| 10 | `Assets/DiaryImages/Materials.meta` |
| 11 | `Assets/DiaryImages/New Folder.meta` |
| 12 | `Assets/Player/Materials.meta` |

> 6 mục cuối (7–12) ban đầu được di chuyển sang thư mục đích dưới `Assets/_Project/` với GUID giữ nguyên,
> nhưng sau đó thư mục đích rỗng bị xoá theo yêu cầu — xem mục 9.2.

## 8. Chi tiết thư mục

### 8.1. Thư mục giữ nguyên GUID (158 thư mục)

> Trong 158 thư mục dưới đây, **6 thư mục đích rỗng đã bị xoá** sau refactor (xem 8.3 và 9.2).
> Các dòng tương ứng vẫn được giữ lại để tra cứu lịch sử di chuyển.

| # | Thư mục cũ | Thư mục mới |
| ---: | --- | --- |
| 1 | `Assets/_0317/Prefabs` | `Assets/_Project/Prefabs` |
| 2 | `Assets/_0317/Prefabs/Environment` | `Assets/_Project/Prefabs/Environment` |
| 3 | `Assets/_0317/Prefabs/Gameplay` | `Assets/_Project/Prefabs/Gameplay` |
| 4 | `Assets/_0317/Scripts` | `Assets/_Project/Scripts` |
| 5 | `Assets/_0317/Scripts/Gameplay` | `Assets/_Project/Scripts/Gameplay` |
| 6 | `Assets/Audio` | `Assets/_Project/Audio` |
| 7 | `Assets/BuddhistFuneralAltar` | `Assets/_Unknown/BuddhistFuneralAltar` |
| 8 | `Assets/BuddhistFuneralAltar/textures` | `Assets/_Unknown/BuddhistFuneralAltar/textures` |
| 9 | `Assets/bupbe` | `Assets/_Unknown/bupbe` |
| 10 | `Assets/CacLoaiBay` | `Assets/_Unknown/CacLoaiBay` |
| 11 | `Assets/CacLoaiBay/3DauAn` | `Assets/_Unknown/CacLoaiBay/3DauAn` |
| 12 | `Assets/CacLoaiBay/3DauAn/eye` | `Assets/_Unknown/CacLoaiBay/3DauAn/eye` |
| 13 | `Assets/CacLoaiBay/3DauAn/eye/Materials` | `Assets/_Unknown/CacLoaiBay/3DauAn/eye/Materials` |
| 14 | `Assets/CacLoaiBay/3DauAn/Fire` | `Assets/_Unknown/CacLoaiBay/3DauAn/Fire` |
| 15 | `Assets/CacLoaiBay/3DauAn/Moon` | `Assets/_Unknown/CacLoaiBay/3DauAn/Moon` |
| 16 | `Assets/CacLoaiBay/Bandage` | `Assets/_Unknown/CacLoaiBay/Bandage` |
| 17 | `Assets/CacLoaiBay/Bandage/Materials` | `Assets/_Unknown/CacLoaiBay/Bandage/Materials` |
| 18 | `Assets/CacLoaiBay/Bandage/textures` | `Assets/_Unknown/CacLoaiBay/Bandage/textures` |
| 19 | `Assets/CacLoaiBay/BayGau` | `Assets/_Unknown/CacLoaiBay/BayGau` |
| 20 | `Assets/CacLoaiBay/BayGau/beartrap` | `Assets/_Unknown/CacLoaiBay/BayGau/beartrap` |
| 21 | `Assets/DiaryImages/Materials` | `Assets/_Project/Image/DiaryImages/Materials` |
| 22 | `Assets/DiaryImages/New Folder` | `Assets/_Project/Image/DiaryImages/New Folder` |
| 23 | `Assets/FpsHorrorKit` | `Assets/_ThirdParty/FpsHorrorKit` |
| 24 | `Assets/FpsHorrorKit/Animations` | `Assets/_ThirdParty/FpsHorrorKit/Animations` |
| 25 | `Assets/FpsHorrorKit/Audio` | `Assets/_ThirdParty/FpsHorrorKit/Audio` |
| 26 | `Assets/FpsHorrorKit/CameraSettings` | `Assets/_ThirdParty/FpsHorrorKit/CameraSettings` |
| 27 | `Assets/FpsHorrorKit/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Materials` |
| 28 | `Assets/FpsHorrorKit/Materials/DecalMaterials` | `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials` |
| 29 | `Assets/FpsHorrorKit/Materials/DecalMaterials/TextDecals` | `Assets/_ThirdParty/FpsHorrorKit/Materials/DecalMaterials/TextDecals` |
| 30 | `Assets/FpsHorrorKit/Materials/ExorcismCircle` | `Assets/_ThirdParty/FpsHorrorKit/Materials/ExorcismCircle` |
| 31 | `Assets/FpsHorrorKit/Materials/Interactive` | `Assets/_ThirdParty/FpsHorrorKit/Materials/Interactive` |
| 32 | `Assets/FpsHorrorKit/Models` | `Assets/_ThirdParty/FpsHorrorKit/Models` |
| 33 | `Assets/FpsHorrorKit/Models/Carpet` | `Assets/_ThirdParty/FpsHorrorKit/Models/Carpet` |
| 34 | `Assets/FpsHorrorKit/Models/CellarWalls` | `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls` |
| 35 | `Assets/FpsHorrorKit/Models/CellarWalls/MatAndTex` | `Assets/_ThirdParty/FpsHorrorKit/Models/CellarWalls/MatAndTex` |
| 36 | `Assets/FpsHorrorKit/Models/Doors` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors` |
| 37 | `Assets/FpsHorrorKit/Models/Doors/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Materials` |
| 38 | `Assets/FpsHorrorKit/Models/Doors/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Doors/Textures` |
| 39 | `Assets/FpsHorrorKit/Models/Floors` | `Assets/_ThirdParty/FpsHorrorKit/Models/Floors` |
| 40 | `Assets/FpsHorrorKit/Models/Furnitures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures` |
| 41 | `Assets/FpsHorrorKit/Models/Furnitures/Lights` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Lights` |
| 42 | `Assets/FpsHorrorKit/Models/Furnitures/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Materials` |
| 43 | `Assets/FpsHorrorKit/Models/Furnitures/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Furnitures/Textures` |
| 44 | `Assets/FpsHorrorKit/Models/ModulerParts` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts` |
| 45 | `Assets/FpsHorrorKit/Models/ModulerParts/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Materials` |
| 46 | `Assets/FpsHorrorKit/Models/ModulerParts/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/ModulerParts/Textures` |
| 47 | `Assets/FpsHorrorKit/Models/NewModels` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels` |
| 48 | `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx` |
| 49 | `Assets/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Camera_01_2k.fbx/textures` |
| 50 | `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx` |
| 51 | `Assets/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/Lantern_01_2k.fbx/textures` |
| 52 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx` |
| 53 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Materials` |
| 54 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/Prefabs` |
| 55 | `Assets/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/modular_industrial_pipes_01_2k.fbx/textures` |
| 56 | `Assets/FpsHorrorKit/Models/NewModels/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/textures` |
| 57 | `Assets/FpsHorrorKit/Models/NewModels/UtilityBox` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox` |
| 58 | `Assets/FpsHorrorKit/Models/NewModels/UtilityBox/textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/NewModels/UtilityBox/textures` |
| 59 | `Assets/FpsHorrorKit/Models/Props` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props` |
| 60 | `Assets/FpsHorrorKit/Models/Props/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Materials` |
| 61 | `Assets/FpsHorrorKit/Models/Props/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Models/Props/Textures` |
| 62 | `Assets/FpsHorrorKit/Models/Stairs` | `Assets/_ThirdParty/FpsHorrorKit/Models/Stairs` |
| 63 | `Assets/FpsHorrorKit/Png` | `Assets/_ThirdParty/FpsHorrorKit/Png` |
| 64 | `Assets/FpsHorrorKit/Png/Blood` | `Assets/_ThirdParty/FpsHorrorKit/Png/Blood` |
| 65 | `Assets/FpsHorrorKit/Png/Blood/Materials` | `Assets/_ThirdParty/FpsHorrorKit/Png/Blood/Materials` |
| 66 | `Assets/FpsHorrorKit/Png/Item` | `Assets/_ThirdParty/FpsHorrorKit/Png/Item` |
| 67 | `Assets/FpsHorrorKit/Png/Papers` | `Assets/_ThirdParty/FpsHorrorKit/Png/Papers` |
| 68 | `Assets/FpsHorrorKit/Png/ScriptIcons` | `Assets/_ThirdParty/FpsHorrorKit/Png/ScriptIcons` |
| 69 | `Assets/FpsHorrorKit/Png/UI` | `Assets/_ThirdParty/FpsHorrorKit/Png/UI` |
| 70 | `Assets/FpsHorrorKit/Prefabs` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs` |
| 71 | `Assets/FpsHorrorKit/Prefabs/Cellar` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Cellar` |
| 72 | `Assets/FpsHorrorKit/Prefabs/Decals` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Decals` |
| 73 | `Assets/FpsHorrorKit/Prefabs/Doors` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Doors` |
| 74 | `Assets/FpsHorrorKit/Prefabs/Floors` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Floors` |
| 75 | `Assets/FpsHorrorKit/Prefabs/Furnitures` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures` |
| 76 | `Assets/FpsHorrorKit/Prefabs/Furnitures/Lights` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Furnitures/Lights` |
| 77 | `Assets/FpsHorrorKit/Prefabs/InteractablePrefabs` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/InteractablePrefabs` |
| 78 | `Assets/FpsHorrorKit/Prefabs/ModularParts` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModularParts` |
| 79 | `Assets/FpsHorrorKit/Prefabs/ModulerWalls` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/ModulerWalls` |
| 80 | `Assets/FpsHorrorKit/Prefabs/Player` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Player` |
| 81 | `Assets/FpsHorrorKit/Prefabs/Props` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Props` |
| 82 | `Assets/FpsHorrorKit/Prefabs/Systems` | `Assets/_ThirdParty/FpsHorrorKit/Prefabs/Systems` |
| 83 | `Assets/FpsHorrorKit/Scenes` | `Assets/_ThirdParty/FpsHorrorKit/Scenes` |
| 84 | `Assets/FpsHorrorKit/Scenes/InventorySystemScene` | `Assets/_ThirdParty/FpsHorrorKit/Scenes/InventorySystemScene` |
| 85 | `Assets/FpsHorrorKit/Scenes/MainScene` | `Assets/_ThirdParty/FpsHorrorKit/Scenes/MainScene` |
| 86 | `Assets/FpsHorrorKit/Scripts` | `Assets/_ThirdParty/FpsHorrorKit/Scripts` |
| 87 | `Assets/FpsHorrorKit/Scripts/DialogueSystems` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems` |
| 88 | `Assets/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/DialogueSystems/Dialogues` |
| 89 | `Assets/FpsHorrorKit/Scripts/Editor` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/Editor` |
| 90 | `Assets/FpsHorrorKit/Scripts/FpsController` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/FpsController` |
| 91 | `Assets/FpsHorrorKit/Scripts/InteractSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem` |
| 92 | `Assets/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InteractSystem/InteractableObjects` |
| 93 | `Assets/FpsHorrorKit/Scripts/InventorySyste` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste` |
| 94 | `Assets/FpsHorrorKit/Scripts/InventorySyste/Items` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/InventorySyste/Items` |
| 95 | `Assets/FpsHorrorKit/Scripts/PhotoCaptureSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/PhotoCaptureSystem` |
| 96 | `Assets/FpsHorrorKit/Scripts/Systems` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/Systems` |
| 97 | `Assets/FpsHorrorKit/Scripts/TrapSystem` | `Assets/_ThirdParty/FpsHorrorKit/Scripts/TrapSystem` |
| 98 | `Assets/FpsHorrorKit/Shaders` | `Assets/_ThirdParty/FpsHorrorKit/Shaders` |
| 99 | `Assets/FpsHorrorKit/TextMesh Pro` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro` |
| 100 | `Assets/FpsHorrorKit/TextMesh Pro/Fonts` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Fonts` |
| 101 | `Assets/FpsHorrorKit/TextMesh Pro/Resources` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources` |
| 102 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Fonts & Materials` |
| 103 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Sprite Assets` |
| 104 | `Assets/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Resources/Style Sheets` |
| 105 | `Assets/FpsHorrorKit/TextMesh Pro/Shaders` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Shaders` |
| 106 | `Assets/FpsHorrorKit/TextMesh Pro/Sprites` | `Assets/_ThirdParty/FpsHorrorKit/TextMesh Pro/Sprites` |
| 107 | `Assets/FpsHorrorKit/Textures` | `Assets/_ThirdParty/FpsHorrorKit/Textures` |
| 108 | `Assets/FpsHorrorKit/Textures/Decal` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal` |
| 109 | `Assets/FpsHorrorKit/Textures/Decal/Texts` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Decal/Texts` |
| 110 | `Assets/FpsHorrorKit/Textures/Patterns` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns` |
| 111 | `Assets/FpsHorrorKit/Textures/Patterns/Colorful` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/Colorful` |
| 112 | `Assets/FpsHorrorKit/Textures/Patterns/FavoritePatterns` | `Assets/_ThirdParty/FpsHorrorKit/Textures/Patterns/FavoritePatterns` |
| 113 | `Assets/Free-FlashLight` | `Assets/_ThirdParty/Free-FlashLight` |
| 114 | `Assets/Free-FlashLight/Materials` | `Assets/_ThirdParty/Free-FlashLight/Materials` |
| 115 | `Assets/Free-FlashLight/Models` | `Assets/_ThirdParty/Free-FlashLight/Models` |
| 116 | `Assets/Free-FlashLight/Prefabs` | `Assets/_ThirdParty/Free-FlashLight/Prefabs` |
| 117 | `Assets/Free-FlashLight/Scenes` | `Assets/_ThirdParty/Free-FlashLight/Scenes` |
| 118 | `Assets/Free-FlashLight/Textures` | `Assets/_ThirdParty/Free-FlashLight/Textures` |
| 119 | `Assets/Image` | `Assets/_Project/Image` |
| 120 | `Assets/Image/Art` | `Assets/_Project/Image/Art` |
| 121 | `Assets/Image/Background` | `Assets/_Project/Image/Background` |
| 122 | `Assets/Image/Misc` | `Assets/_Project/Image/Misc` |
| 123 | `Assets/Image/PIcture` | `Assets/_Project/Image/PIcture` |
| 124 | `Assets/Image/Texture` | `Assets/_Project/Image/Texture` |
| 125 | `Assets/MACon` | `Assets/_Project/MACon` |
| 126 | `Assets/MACon/Materials` | `Assets/_Project/MACon/Materials` |
| 127 | `Assets/MaConBay` | `Assets/_Project/MaConBay` |
| 128 | `Assets/MaConBay/Materials` | `Assets/_Project/MaConBay/Materials` |
| 129 | `Assets/MaConBay/music box` | `Assets/_Project/MaConBay/music box` |
| 130 | `Assets/MaConBay/music box/Materials` | `Assets/_Project/MaConBay/music box/Materials` |
| 131 | `Assets/Mame` | `Assets/_Project/Mame` |
| 132 | `Assets/MAMe 1` | `Assets/_Project/MAMe 1` |
| 133 | `Assets/MAMe 1/Materials` | `Assets/_Project/MAMe 1/Materials` |
| 134 | `Assets/Mame/source` | `Assets/_Project/Mame/source` |
| 135 | `Assets/Mame/source/Womann` | `Assets/_Project/Mame/source/Womann` |
| 136 | `Assets/Mame/textures` | `Assets/_Project/Mame/textures` |
| 137 | `Assets/Materials` | `Assets/_Project/Materials` |
| 138 | `Assets/Nen` | `Assets/_Project/Nen` |
| 139 | `Assets/Nen/Materials` | `Assets/_Project/Nen/Materials` |
| 140 | `Assets/OngLam` | `Assets/_Project/OngLam` |
| 141 | `Assets/Player` | `Assets/_Project/Player` |
| 142 | `Assets/Player/Materials` | `Assets/_Project/Player/Materials` |
| 143 | `Assets/Quantai` | `Assets/_Project/Quantai` |
| 144 | `Assets/Rain` | `Assets/_Project/Rain` |
| 145 | `Assets/Rain/Prefab` | `Assets/_Project/Rain/Prefab` |
| 146 | `Assets/RoomAudio` | `Assets/_Project/RoomAudio` |
| 147 | `Assets/Sky` | `Assets/_ThirdParty/Sky` |
| 148 | `Assets/Sky/Cubemaps` | `Assets/_ThirdParty/Sky/Cubemaps` |
| 149 | `Assets/Sky/Cubemaps/Classic` | `Assets/_ThirdParty/Sky/Cubemaps/Classic` |
| 150 | `Assets/Sky/Panoramics` | `Assets/_ThirdParty/Sky/Panoramics` |
| 151 | `Assets/Sky/Panoramics/FS002` | `Assets/_ThirdParty/Sky/Panoramics/FS002` |
| 152 | `Assets/Sky/Panoramics/FS003` | `Assets/_ThirdParty/Sky/Panoramics/FS003` |
| 153 | `Assets/Sky/Panoramics/FS013` | `Assets/_ThirdParty/Sky/Panoramics/FS013` |
| 154 | `Assets/Sky/Panoramics/FS017` | `Assets/_ThirdParty/Sky/Panoramics/FS017` |
| 155 | `Assets/Studio` | `Assets/_Project/Studio` |
| 156 | `Assets/Tree` | `Assets/_ThirdParty/Tree` |
| 157 | `Assets/Tuong` | `Assets/_Project/Tuong` |
| 158 | `Assets/Tuong/Materials` | `Assets/_Project/Tuong/Materials` |

### 8.2. ⚠️ Thư mục bị cấp GUID mới (7 thư mục)

Khi di chuyển thư mục bằng File Explorer (ngoài Unity), Unity có thể tạo lại `.meta` cho thư mục với **GUID mới**.
Điều này chỉ ảnh hưởng nếu có tài nguyên tham chiếu tới **chính thư mục đó** (rất hiếm); đã kiểm tra và **không tìm thấy tham chiếu nào** tới các GUID cũ.

| # | Thư mục cũ | Thư mục mới | GUID cũ | GUID mới |
| ---: | --- | --- | --- | --- |
| 1 | `Assets/AnhFull` | `Assets/_Project/Materials/AnhFull` | `ce3114845535bab4da2f5d983ee50c06` | `26aabf65281c217409f4bc9bb4591403` |
| 2 | `Assets/AnhFull/Materials` | `Assets/_Project` | `e51b1abf830f65643bcf2186f9c40e0e` | `278a5725f5a6fe54b9dad64a3eaa596b` |
| 3 | `Assets/BoXuong` | `Assets/_Project/Image/OngLam` | `b795449fcfb285344855007900506a53` | `572abfbdedfd7d54898177e07e42fccd` |
| 4 | `Assets/Dau_ThienLinh` | `Assets/_Project/Image/Dau_ThienLinh` | `206d63baac031bc48b5ef222eb15da05` | `2c32aed66a191b449ba2f8f799f90f9d` |
| 5 | `Assets/Dau_ThienLinh/Materials` | `Assets/_Project/Image/Texture/Blood` | `7e9f4729277181649ad55d8649ec45a7` | `5f749c9ecf107d240a537c09e3a436de` |
| 6 | `Assets/DiaryImages` | `Assets/_Project/Image/DiaryImages` | `396c1422590f0cf43b4287695392c2f5` | `bae7ca01088c57840843c1ee8a832377` |
| 7 | `Assets/Ghost` | `Assets/_Project/Image/Ghost` | `9e1365f25558c3644a0c1bcf199b4b3c` | `470415dade15fac429d31b6d4f567b92` |

### 8.3. Thư mục đích rỗng đã xoá (6 thư mục)

6 thư mục dưới đây là **đích đến của các thư mục cũ** trong quá trình refactor. Sau khi toàn bộ file bên trong
được chuyển tiếp sang thư mục khác (hoặc không còn file nào), chúng trở thành thư mục rỗng và đã bị xoá
cùng `.meta` theo yêu cầu. Thư mục cũ tương ứng cũng không còn tồn tại.

| # | Thư mục cũ | Thư mục đích đã xoá | GUID (`.meta` cũ) | Tham chiếu tới GUID |
| ---: | --- | --- | --- | --- |
| 1 | `Assets/Image/Art` | `Assets/_Project/Image/Art` | `a56c35f25b6b55b4daa12962ecb2df99` | 0 |
| 2 | `Assets/Image/Background` | `Assets/_Project/Image/Background` | `94ec4c460aa615e4280f299c928a472d` | 0 |
| 3 | `Assets/Image/Misc` | `Assets/_Project/Image/Misc` | `c90a944979826054bac4b6b74e31b7cc` | 0 |
| 4 | `Assets/DiaryImages/Materials` | `Assets/_Project/Image/DiaryImages/Materials` | `b0e0451f4065d4a4284b43df30f78997` | 0 |
| 5 | `Assets/DiaryImages/New Folder` | `Assets/_Project/Image/DiaryImages/New Folder` | `da64774821c2cd144815a022feba4e8c` | 0 |
| 6 | `Assets/Player/Materials` | `Assets/_Project/Player/Materials` | `5ccbeb435bffcb04f9cad52d45193ac7` | 0 |

> Đã kiểm tra: không có file asset/scene/prefab nào trong `Assets/` tham chiếu tới 6 GUID trên.
> Toàn bộ nội dung của các thư mục này đã được chuyển tới nơi khác trước khi xoá (xem mục 5).

## 9. Các trường hợp khác & cảnh báo

### 9.1. Thư mục bị đổi GUID

Xem bảng 8.2. Không phát hiện tham chiếu tới GUID cũ trong `Assets/`, nên **hiện tại không gây lỗi**.
Nếu sau này có asset tham chiếu tới thư mục (ví dụ Addressables group, asset label), cần kiểm tra lại.

### 9.2. Thư mục rỗng đã xoá

Sau khi di chuyển toàn bộ file, 6 thư mục đích dưới đây trở thành **rỗng** và đã được xoá cùng file `.meta`
theo yêu cầu (đã kiểm tra `0 entry` bên trong trước khi xoá):

| # | Thư mục đã xoá | `.meta` đã xoá | GUID | Tham chiếu tới GUID |
| ---: | --- | --- | --- | --- |
| 1 | `Assets/_Project/Image/Art` | `Assets/_Project/Image/Art.meta` | `a56c35f25b6b55b4daa12962ecb2df99` | 0 |
| 2 | `Assets/_Project/Image/Background` | `Assets/_Project/Image/Background.meta` | `94ec4c460aa615e4280f299c928a472d` | 0 |
| 3 | `Assets/_Project/Image/Misc` | `Assets/_Project/Image/Misc.meta` | `c90a944979826054bac4b6b74e31b7cc` | 0 |
| 4 | `Assets/_Project/Image/DiaryImages/Materials` | `Assets/_Project/Image/DiaryImages/Materials.meta` | `b0e0451f4065d4a4284b43df30f78997` | 0 |
| 5 | `Assets/_Project/Image/DiaryImages/New Folder` | `Assets/_Project/Image/DiaryImages/New Folder.meta` | `da64774821c2cd144815a022feba4e8c` | 0 |
| 6 | `Assets/_Project/Player/Materials` | `Assets/_Project/Player/Materials.meta` | `5ccbeb435bffcb04f9cad52d45193ac7` | 0 |

Toàn bộ file asset từng nằm trong các thư mục này đã được chuyển sang nơi khác trước khi xoá (xem mục 5).
Chi tiết GUID và đường dẫn cũ nằm ở mục **8.3**.

### 9.3. Material bị Unity serialize lại (không liên quan đổi tên)

10 file material trong `Assets/Vefects/Free Fire HDRP/_ Extra/Resources/Materials/` bị Unity ghi lại khi import:
`M_Vefects_Extra_Billboard_01/02/04/07/09/10/12/13/14.mat` và `M_Vefects_Extra_Grid_01.mat`.
Thay đổi chủ yếu là thêm field mới của Unity (`m_Parent`, `m_ModifiedSerializedProperties`, `m_LockedProperties`…) — **không phải do refactor**.

### 9.4. Scene chính và thư mục backup

**Kết luận: `Assets/_Backups/Tho_Opening_PrePlay_20261105_1440.unity` là bản sao lưu của scene gameplay chính,
không phải scene gốc đang dùng. Scene chính hiện tại là `Assets/_ThirdParty/FpsHorrorKit/Scenes/Tho.unity`.**

Bằng chứng đã kiểm tra:

| Kiểm tra | Kết quả |
| --- | --- |
| Kích thước / số dòng `_Backups/Tho_Opening_PrePlay_20261105_1440.unity` | 3,787,856 bytes / 99,672 dòng |
| Kích thước / số dòng `_ThirdParty/FpsHorrorKit/Scenes/Tho.unity` (scene đang dùng) | 3,867,068 bytes / 101,762 dòng |
| Mức trùng khớp nội dung | Cùng cấu trúc phòng (`Room_1`–`Room_8`, `Shard`, `Trap_Trigger`, `Den Phong Bua`…); khác ~2,510 dòng do bản backup cũ hơn |
| Nguồn gốc file backup | Được thêm vào commit `471e9a9` (2026-11-05 17:58), cùng lúc với scene `FpsHorrorKit/Scenes/Tho.unity` |
| Timestamp trong tên file | `20261105_1440` = bản chụp ngày **05/11/2026 lúc 14:40** |
| `Assets/OutdoorsScene.unity` | Chỉ 19,310 bytes / 699 dòng; chỉ có `Main Camera`, `Sky and Fog Volume`, `StaticLightingSky`, `Sun` → đây là **scene mẫu HDRP mặc định**, không phải scene gameplay |
| `ProjectSettings/EditorBuildSettings.asset` | Chỉ liệt kê `Assets/OutdoorsScene.unity`; file **không thay đổi từ commit đầu tiên `554b702`** → build settings chưa từng được cập nhật theo scene thật |

Ghi chú:

- `Assets/_Backups/` **không nằm trong danh sách thay đổi refactor** (không bị di chuyển/đổi tên).
- Tên thư mục `_Backups` + timestamp cho thấy đây là bản chụp thủ công để phòng hồi quy, không phải nơi đặt scene chính thức.
- Vì scene chính đang nằm trong thư mục third-party (`_ThirdParty/FpsHorrorKit/Scenes/`), nên tách ra `Assets/Scene/`
  hoặc `Assets/_Project/Scenes/` và cập nhật `EditorBuildSettings.asset`. **Người dùng sẽ tự thực hiện sau** (mục 12.1).

### 9.5. Tên file chứa ký tự Unicode

Một số file giữ nguyên tên gốc tiếng Việt / tiếng Nga (ví dụ `Sao cửa lại bị -9f859c87f6_9c87f6.mp3`, `лес3 crtn vjlba.fbx`).
Git on Windows có thể hiển thị dạng escape (`\320\273...`). **Không đổi tên các file này** trừ khi có lý do, vì dễ gây lỗi encoding khi commit.

### 9.6. Trùng GUID có sẵn từ trước

`Assets/Settings/SkyandFogSettingsProfile.asset.meta` có GUID trùng với sample trong package ShaderGraph (`Library/PackageCache/...`).
Đây là vấn đề **có từ trước**, không do refactor, nhưng nên kiểm tra nếu gặp lỗi import bất thường.

### 9.7. Mục đích thư mục `Assets/_Unknown/`

`Assets/_Unknown/` là **thư mục tạm** do người thực hiện refactor tạo ra, dùng để gom các asset
**chưa xác định được là gì / dùng để làm gì**. Tên "Unknown" là cố ý: đây không phải phân loại cuối cùng,
mà là nơi giữ an toàn để **không xoá nhầm** và để phân loại sau.

| Thư mục con | Số file asset | Ghi chú |
| --- | ---: | --- |
| `Assets/_Unknown/CacLoaiBay` | 25 | 3DauAn, Bandage, BayGau… |
| `Assets/_Unknown/BuddhistFuneralAltar` | 6 | Gồm thư mục con `textures` |
| `Assets/_Unknown/bupbe` | 3 | |
| **Tổng** | **34** | Không asset nào bị xoá; GUID giữ nguyên |

## 10. Khôi phục / rollback

Vì **toàn bộ refactor chưa được commit**, mọi đường dẫn cũ vẫn còn trong `HEAD` (`009edb1`). Có thể khôi phục như sau:

### 10.1. Khôi phục toàn bộ về trạng thái commit gần nhất

```bash
# 1. Xem lại những gì sẽ mất
git status

# 2. Khôi phục file đã xoá/đã sửa (đưa file về đường dẫn cũ)
git restore --source=HEAD -- .

# 3. Xoá các file MỚI ở đường dẫn mới (untracked) — CẨN THẬN, không thể hoàn tác
git clean -fd -- Assets
```

> Trước khi chạy `git clean`, nên tạo nhánh/commit tạm để giữ lại trạng thái hiện tại:
> `git stash push -u -m "refactor WIP"` hoặc `git checkout -b backup/refactor-wip && git add -A && git commit -m "wip refactor"`.

### 10.2. Khôi phục một file cụ thể

1. Tìm file trong **mục 5** để biết đường dẫn cũ/mới.
2. Khôi phục file cũ: `git checkout HEAD -- "<đường_dẫn_cũ>"` (hoặc `git show HEAD:"<path>" > <path>`).
3. `.meta` đi kèm cũng phải khôi phục cùng lúc để giữ GUID.

### 10.3. Kiểm tra nhanh sau khi khôi phục

```bash
git status
git diff --stat
```

Nếu Unity báo Missing: mở file `.meta` của asset gốc và đối chiếu `guid` với giá trị đang tham chiếu trong scene/prefab.

## 11. Thay đổi ngoài `Assets/` (chỉ ghi nhận, không đi sâu)

| File | Thay đổi |
| --- | --- |
| `.gitignore` | Được viết lại/chuẩn hoá (thêm rule Unity, IDE, build, Addressables…) |
| `Packages/manifest.json` | Thêm dependency `com.unity.2d.sprite` (`1.0.0`) |
| `Packages/packages-lock.json` | Thêm entry tương ứng cho `com.unity.2d.sprite` |
| `ProjectSettings/*` | `git status` báo thay đổi nhưng `git diff` rỗng — **chỉ khác line-ending (CRLF/LF)**, không đổi nội dung |

## 12. Quyết định đã chốt & câu hỏi còn lại

### 12.1. Đã chốt

| # | Vấn đề | Quyết định |
| ---: | --- | --- |
| 1 | Mục đích `Assets/_Unknown/` | Thư mục tạm cho asset chưa xác định được; giữ nguyên và phân loại sau (mục 9.7) |
| 2 | 6 thư mục rỗng | Đã **xoá cả thư mục và `.meta`** (mục 9.2 và 8.3) |
| 3 | Commit | **Chưa commit** — việc di chuyển file chưa xong; toàn bộ thay đổi vẫn ở working tree (mục 10) |
| 4 | `Assets/_Backups/` | Là bản sao lưu scene chính, không phải scene đang dùng (mục 9.4) |
| 5 | Di chuyển scene chính `Tho.unity` | **Người dùng sẽ tự làm sau** — chưa di chuyển trong lần refactor này |
| 6 | Bản backup `Tho_Opening_PrePlay_20261105_1440.unity` | **Người dùng sẽ tự xử lý sau** — hiện vẫn giữ nguyên |
| 7 | `Assets/OutdoorsScene.unity` | **Giữ lại làm scene test** (ánh sáng/sky), không xoá |

### 12.2. Còn để ngỏ

1. Chưa chốt đích đến cuối cùng của scene chính (`Assets/Scene/` hay `Assets/_Project/Scenes/`) —
   khi di chuyển cần giữ nguyên file `.meta` (GUID `8c5c3c12…` của `_Backups`, GUID của `Tho.unity`) và cập nhật
   `ProjectSettings/EditorBuildSettings.asset`.
2. Chưa chốt có đổi tên các thư mục/tài nguyên tiếng Việt (`OngLam`, `MaConBay`, `Dau_ThienLinh`, `Tuong`, `Nen`…) sang tiếng Anh hay không.

