# TaskAPI test envanteri ve kapsamı

Tarih: 8 Ekim 2026  
Kaynak: TaskAPI.API/Tests ve TaskAPI.Application/Tests altındaki mevcut C# dosyaları.  
Bu incelemede testler çalıştırılmadı ve uygulama kodu değiştirilmedi.

## Sonuç bilgisi ve nasıl okunur

Kullanıcının bildirdiği son sonuç: **154 testin 154’ü başarılı**. Bu sayı kaynakta bulunan **82 test metodunun 154 ayrı senaryosuna** karşılık gelir. Bu belge test sonuç raporu değil, kaynak kodun neyi kontrol ettiğini gösteren envanterdir.

- **Fact (tek senaryolu test):** bir metot bir test olarak çalışır.
- **Theory (veriyle tekrarlanan test):** aynı metot her InlineData girdisiyle ayrı çalışır; aşağıda her girdi ayrı numaralandırılmıştır.
- **Assert (beklenti kontrolü):** gelen sonuç ile beklenen sonucu karşılaştırır.
- **Mock (kontrollü taklit):** gerçek repository/service yerine testin ayarladığı sonucu veya hatayı üretir. Mock ile geçen bir test tek başına gerçek SQL Server/HTTP akışını doğrulamaz.
- **InnerException (iç hata):** sarılmış hatanın önceki nedenini korur.
- **StackTrace (çağrı izi):** hata oluştuğunda geçtiği metotları gösterir.
- **CorrelationId (takip numarası):** bu testlerde ErrorModel’in ürettiği hata kimliğidir; bütün istek boyunca aynı kimliğin kullanıldığı ayrıca test edilmemiştir.

### Kaynakta görülen beklenti farkı

Application’daki `addTaskWithEmptyTitleTest` ve `AddTaskWithPastDueDateTest` **400** bekliyor. API’daki karşılıkları ve şu anki TaskService aynı kurallar için **500** kullanıyor. Dolayısıyla “154 geçti” bilgisini mevcut dosyaların tamamına ait yeniden doğrulanmış bir sonuç olarak kabul etmiyoruz. Burada sayılan senaryoların beklentileri dosyalarda yazıldığı haliyle belgelenmiştir. Test keşif listesi/sonuç çıktısı olmadan hangi çalışma kapsamının bu eski testleri dahil ettiğini kesinleştirmiyoruz.

## Katmanlar ve test yöntemi

| Alan | Kullanılan gerçek bileşen | Taklit veya test ortamı | Kanıtın sınırı |
|---|---|---|---|
| ErrorModel / TaskApiException | Model ve exception constructor’ları | Örnek veriler ve hatalar | Gerçek iş isteği yok |
| ErrorHandler | Middleware Invoke ve JSON yazımı | DefaultHttpContext, MemoryStream, test next delegate’i | Program.cs middleware sırası ve routing çalışmaz |
| MethodExecutor | Dört executor metodu | Toplanan loglar ve test HTTP context’i | Gerçek log hedefi/dosya/izleme sistemi kontrol edilmez |
| TaskController / AuthController | Controller metotları | Mock service | Model binding, JSON istek okuma ve gerçek HTTP sunucusu çalışmaz |
| TaskService / AccountService | Service metotları | Mock repository; ilgili testlerde mock mapper/hasher | Gerçek kayıt ve hash işlemi bu testlerde yapılmaz |
| Mapping | Gerçek AutoMapper profili | Örnek DTO/entity | Service ve veritabanı birlikte çalışmaz |
| Repository | Gerçek EF Core context ve repository | Bellekte SQLite | SQL Server migration ve üretim bağlantısını doğrulamaz |
| ConfigurationProvider | Gerçek provider | Bellekte configuration | Gerçek User Secrets dosyasını okumaz |
| PasswordHasher | Gerçek hash/verify algoritması | Mock pepper provider | Gerçek secret deposu ve dağıtım ortamı kontrol edilmez |
| User | Gerçek constructor | Örnek alan değerleri | Controller validation’ı kontrol edilmez |

## Sayısal dağılım

| Test dosyası ve sınıf | Metot sayısı | Senaryo sayısı |
|---|---:|---:|
| TaskAPI.API/Tests/AccountServiceTests.cs — AccountServiceTests | 7 | 7 |
| TaskAPI.API/Tests/ControllerTests.cs — TaskControllerTests | 4 | 14 |
| TaskAPI.API/Tests/ControllerTests.cs — AuthControllerTests | 4 | 6 |
| TaskAPI.API/Tests/ErrorSystemTests.cs — ErrorModelTests | 2 | 5 |
| TaskAPI.API/Tests/ErrorSystemTests.cs — TaskApiExceptionTests | 1 | 4 |
| TaskAPI.API/Tests/ErrorSystemTests.cs — ErrorHandlerTests | 3 | 9 |
| TaskAPI.API/Tests/ErrorSystemTests.cs — MethodExecutorTests | 9 | 29 |
| TaskAPI.API/Tests/MappingAndDomainTests.cs — MappingTests | 4 | 4 |
| TaskAPI.API/Tests/MappingAndDomainTests.cs — UserTests | 2 | 6 |
| TaskAPI.API/Tests/RepositoryTests.cs — RepositoryTests | 12 | 12 |
| TaskAPI.API/Tests/SecurityTests.cs — ConfigurationProviderTests | 4 | 6 |
| TaskAPI.API/Tests/SecurityTests.cs — PasswordHasherTests | 11 | 19 |
| TaskAPI.API/Tests/TaskServiceTests.cs — TaskServiceTests | 15 | 26 |
| TaskAPI.Application/Tests/ErrorHandlerTests.cs — ErrorHandlerTests | 1 | 4 |
| TaskAPI.Application/Tests/TaskServiceTests.cs — TaskServiceTests | 3 | 3 |
| **Toplam** | **82** | **154** |

## Her testin kontrol ettiği davranış

Parametre sütunu InlineData girdisini dosyadaki biçimiyle gösterir. “—” tek senaryolu test anlamındadır. Aynı metot farklı girdilerle listelenmişse ayrı çalıştırılan testlerdir.

### TaskAPI.API/Tests/AccountServiceTests.cs — AccountServiceTests

#### Login_CorrectPassword_ReturnsTrue

Kaynak: [AccountServiceTests.Login_CorrectPassword_ReturnsTrue](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:25)

Kullanıcı bulunduğunda ve hasher şifreyi doğru kabul ettiğinde Login true döndürür; Verify bir kez çağrılır. Gerçek hash algoritması bu testte kullanılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 1 | `—` |

#### Login_MissingUser_DoesNotVerifyPassword

Kaynak: [AccountServiceTests.Login_MissingUser_DoesNotVerifyPassword](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:36)

Kullanıcı bulunamazsa “Invalid username or password.” mesajıyla Exception fırlatılır; şifre doğrulaması hiç çağrılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 2 | `—` |

#### Login_WrongPassword_ThrowsCurrentAuthenticationError

Kaynak: [AccountServiceTests.Login_WrongPassword_ThrowsCurrentAuthenticationError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:47)

Kullanıcı bulunmasına rağmen şifre doğrulaması false ise aynı giriş hatası fırlatılır. Bu service testi HTTP 401 kontrol etmez.

| No | Ayrı senaryo / parametre |
|---|---|
| 3 | `—` |

#### Login_RepositoryFailure_IsNotHidden

Kaynak: [AccountServiceTests.Login_RepositoryFailure_IsNotHidden](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:58)

Kullanıcı sorgusunda oluşan InvalidOperationException aynı nesne olarak dışarı aktarılır.

| No | Ayrı senaryo / parametre |
|---|---|
| 4 | `—` |

#### Login_HasherFailure_IsNotHidden

Kaynak: [AccountServiceTests.Login_HasherFailure_IsNotHidden](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:69)

Verify sırasında oluşan FormatException aynı nesne olarak dışarı aktarılır.

| No | Ayrı senaryo / parametre |
|---|---|
| 5 | `—` |

#### Register_NewUser_ShouldHashPasswordAndSaveUser

Kaynak: [AccountServiceTests.Register_NewUser_ShouldHashPasswordAndSaveUser](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:81)

Yeni kullanıcı için Hash bir kez çağrılır; AddAsync bir kez doğru username ve üretilen hash ile çağrılır; düz şifre kaydedilen PasswordHash olmaz; Register true döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 6 | `—` |

#### Register_ExistingUser_ShouldReturnFalseWithoutSaving

Kaynak: [AccountServiceTests.Register_ExistingUser_ShouldReturnFalseWithoutSaving](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/AccountServiceTests.cs:99)

Kullanıcı zaten varsa Register false döndürür; şifre hashlenmez ve kullanıcı eklenmez.

| No | Ayrı senaryo / parametre |
|---|---|
| 7 | `—` |

### TaskAPI.API/Tests/ControllerTests.cs — TaskControllerTests

#### SuccessfulOperations_Return200AndExpectedResult

Kaynak: [TaskControllerTests.SuccessfulOperations_Return200AndExpectedResult](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:52)

Service başarılı sonuç verdiğinde controller OkObjectResult/200 döndürür. Get/Create/Update aynı DTO’yu, Delete true değerini, GetAll aynı listeyi verir.

| No | Ayrı senaryo / parametre |
|---|---|
| 8 | `"Get"` |
| 9 | `"Create"` |
| 10 | `"Update"` |
| 11 | `"Delete"` |
| 12 | `"GetAll"` |

#### MissingServiceResult_UsesExistingArgumentExceptionChecks

Kaynak: [TaskControllerTests.MissingServiceResult_UsesExistingArgumentExceptionChecks](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:72)

Service DTO/liste için null veya Delete için false döndürürse controller belirtilen mesajla ArgumentException fırlatır. HTTP cevabı burada çalıştırılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 13 | `"Get", "Task not found."` |
| 14 | `"Create", "Failed to create task."` |
| 15 | `"Update", "Task not found."` |
| 16 | `"Delete", "Task not found."` |
| 17 | `"GetAll", "No tasks found."` |

#### Get_UnexpectedServiceError_IsWrapped

Kaynak: [TaskControllerTests.Get_UnexpectedServiceError_IsWrapped](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:99)

Service kaynaklı beklenmeyen hata executor tarafından TaskApiException ile sarılır; INTERNAL_SERVER_ERROR/500, “Get could not be completed.” ve orijinal InnerException korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 18 | `—` |

#### ControllerError_ReachesMiddlewareWithOriginalInformation

Kaynak: [TaskControllerTests.ControllerError_ReachesMiddlewareWithOriginalInformation](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:116)

Mock service’in özel hatası controller/executor üzerinden ErrorHandler’a ulaşır; HTTP durumu, JSON errorCode, mesaj ve aynı correlationId korunur. Gerçek HTTP sunucusu başlatılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 19 | `ErrorCodes.TaskNotFound, 404` |
| 20 | `ErrorCodes.InvalidTaskFormat, 500` |
| 21 | `ErrorCodes.BadRequest, 400` |

### TaskAPI.API/Tests/ControllerTests.cs — AuthControllerTests

#### Register_ReturnsSuccessOrConflict

Kaynak: [AuthControllerTests.Register_ReturnsSuccessOrConflict](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:157)

Service true döndürürse success/result true ve error null; false döndürürse HTTP 409, success false, result null, CONFLICT ve model StatusCode 409 kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 22 | `true` |
| 23 | `false` |

#### Login_Success_ReturnsSuccessfulResponse

Kaynak: [AuthControllerTests.Login_Success_ReturnsSuccessfulResponse](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:185)

Service true döndürdüğünde HTTP 200 ve ResponseModel<bool> içinde success/result true, error null kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 24 | `—` |

#### Login_Failure_HttpStatusAndModelStatusShouldBothBe401

Kaynak: [AuthControllerTests.Login_Failure_HttpStatusAndModelStatusShouldBothBe401](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:199)

Mock service false döndürdüğünde HTTP kodu ve ErrorModel.StatusCode birlikte 401 olur; success false, result null ve UNAUTHORIZED kontrol edilir. Gerçek AccountService’in yanlış şifre davranışını bu test bağlamaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 25 | `—` |

#### UnexpectedAccountError_IsWrappedByController

Kaynak: [AuthControllerTests.UnexpectedAccountError_IsWrappedByController](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ControllerTests.cs:218)

Register/Login sırasında mock service’in beklenmeyen hatası 500 ile sarılır; orijinal hata InnerException olarak ve işlem adı mesajda korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 26 | `"Register"` |
| 27 | `"Login"` |

### TaskAPI.API/Tests/ErrorSystemTests.cs — ErrorModelTests

#### Constructors_CreateExpectedErrorInformation

Kaynak: [ErrorModelTests.Constructors_CreateExpectedErrorInformation](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:17)

ErrorModel constructor’ının kodu, HTTP durumunu, mesajı, isteğe bağlı açıklamayı, geçerli GUID takip numarasını ve test sırasında üretilen zamanı doğru atadığı kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 28 | `0` |
| 29 | `1` |
| 30 | `2` |
| 31 | `3` |

#### SeparateErrors_HaveDifferentCorrelationIds

Kaynak: [ErrorModelTests.SeparateErrors_HaveDifferentCorrelationIds](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:59)

Ayrı oluşturulan iki ErrorModel farklı takip numaraları üretir.

| No | Ayrı senaryo / parametre |
|---|---|
| 32 | `—` |

### TaskAPI.API/Tests/ErrorSystemTests.cs — TaskApiExceptionTests

#### Constructors_PreserveMessageCodeStatusAndInnerError

Kaynak: [TaskApiExceptionTests.Constructors_PreserveMessageCodeStatusAndInnerError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:75)

TaskApiException constructor’ının Exception.Message ile errorModel.Message değerlerini eşit tuttuğu, kod/durum/metadata bilgilerini ve verilmişse aynı InnerException nesnesini koruduğu kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 33 | `0` |
| 34 | `1` |
| 35 | `2` |
| 36 | `3` |

### TaskAPI.API/Tests/ErrorSystemTests.cs — ErrorHandlerTests

#### CustomError_PreservesItsEntireModel

Kaynak: [ErrorHandlerTests.CustomError_PreservesItsEntireModel](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:127)

ErrorHandler özel hatayı JSON hata cevabına dönüştürür: HTTP kodu, JSON content type, success false/result null ve modelin kod/durum/mesaj/açıklama/takip numarası/zamanı korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 37 | `ErrorCodes.InvalidTaskFormat, 500` |
| 38 | `ErrorCodes.BadRequest, 400` |
| 39 | `ErrorCodes.TaskNotFound, 404` |
| 40 | `ErrorCodes.Unauthorized, 401` |
| 41 | `ErrorCodes.Conflict, 409` |
| 42 | `ErrorCodes.InternalServerError, 500` |

#### StandardErrors_AreConvertedToExpectedResponses

Kaynak: [ErrorHandlerTests.StandardErrors_AreConvertedToExpectedResponses](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:162)

ArgumentException 400/BAD_REQUEST, InvalidOperationException 500/INTERNAL_SERVER_ERROR olarak dönüştürülür; mesaj, geçerli correlationId ve atanmış timestamp kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 43 | `true` |
| 44 | `false` |

#### SuccessfulRequest_IsNotReplacedWithAnError

Kaynak: [ErrorHandlerTests.SuccessfulRequest_IsNotReplacedWithAnError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:190)

Sonraki işlem hata vermediğinde middleware başarılı 201 durumunu ve yazılmış cevap metnini değiştirmez.

| No | Ayrı senaryo / parametre |
|---|---|
| 45 | `—` |

### TaskAPI.API/Tests/ErrorSystemTests.cs — MethodExecutorTests

#### SuccessfulOperation_RunsOnceAndLogsStartAndCompletion

Kaynak: [MethodExecutorTests.SuccessfulOperation_RunsOnceAndLogsStartAndCompletion](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:252)

İşlem yalnızca bir kez çalışır; başlangıç ve başarı için iki Information kaydı oluşur; başlangıç kaydında test istek yolu bulunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 46 | `"ExecuteAsync"` |
| 47 | `"Execute"` |
| 48 | `"RunAsync"` |
| 49 | `"Run"` |

#### ExecuteMethods_ReturnTheOperationResult

Kaynak: [MethodExecutorTests.ExecuteMethods_ReturnTheOperationResult](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:279)

Execute veya ExecuteAsync işin ürettiği 42 değerini aynen döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 50 | `true` |
| 51 | `false` |

#### ArgumentError_IsLoggedAndRethrownWithoutReplacement

Kaynak: [MethodExecutorTests.ArgumentError_IsLoggedAndRethrownWithoutReplacement](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:296)

ArgumentException aynı nesne olarak tekrar fırlatılır; Warning kaydı oluşur ve başarı kaydı yazılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 52 | `"ExecuteAsync"` |
| 53 | `"Execute"` |
| 54 | `"RunAsync"` |
| 55 | `"Run"` |

#### CustomError_IsNotWrappedAgain

Kaynak: [MethodExecutorTests.CustomError_IsNotWrappedAgain](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:313)

TaskApiException yeniden sarılmaz; aynı nesne, 404 durumu ve exception nesnesini taşıyan log kaydı korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 56 | `"ExecuteAsync"` |
| 57 | `"Execute"` |
| 58 | `"RunAsync"` |
| 59 | `"Run"` |

#### UnexpectedError_UsesEachMethodsCurrentBehavior

Kaynak: [MethodExecutorTests.UnexpectedError_UsesEachMethodsCurrentBehavior](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:330)

ExecuteAsync beklenmeyen hatayı INTERNAL_SERVER_ERROR/500 ile sarar ve orijinal hatayı InnerException’da tutar. Execute, RunAsync ve Run mevcut davranışları gereği orijinal InvalidOperationException’ı aktarır.

| No | Ayrı senaryo / parametre |
|---|---|
| 60 | `"ExecuteAsync"` |
| 61 | `"Execute"` |
| 62 | `"RunAsync"` |
| 63 | `"Run"` |

#### MissingHttpContext_PreventsOperationFromRunning

Kaynak: [MethodExecutorTests.MissingHttpContext_PreventsOperationFromRunning](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:357)

Aktif HTTP isteği yoksa “No active HTTP request found” hatası oluşur; işlem çalışmaz ve log listesi boş kalır.

| No | Ayrı senaryo / parametre |
|---|---|
| 64 | `"ExecuteAsync"` |
| 65 | `"Execute"` |
| 66 | `"RunAsync"` |
| 67 | `"Run"` |

#### ExecuteAsync_LogsEveryInnerExceptionLayer

Kaynak: [MethodExecutorTests.ExecuteAsync_LogsEveryInnerExceptionLayer](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:384)

Özel hatanın InnerException zincirindeki her seviye için bir kayıt oluşur; derinlik numarası ve ilgili katman mesajı doğru sırada kontrol edilir. Bunlar hata zincirinin seviyeleridir; mimari katman sayısı değildir.

| No | Ayrı senaryo / parametre |
|---|---|
| 68 | `0` |
| 69 | `1` |
| 70 | `3` |
| 71 | `10` |
| 72 | `50` |

#### NestedExecutors_WrapUnexpectedErrorOnlyOnce

Kaynak: [MethodExecutorTests.NestedExecutors_WrapUnexpectedErrorOnlyOnce](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:413)

İç içe Service/Controller executor çağrılarında hata yalnızca ilk noktada sarılır; orijinal hata doğrudan InnerException kalır ve mesaj Service işlemine aittir.

| No | Ayrı senaryo / parametre |
|---|---|
| 73 | `—` |

#### Wrapping_PreservesOriginalCallPath

Kaynak: [MethodExecutorTests.Wrapping_PreservesOriginalCallPath](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/ErrorSystemTests.cs:431)

Yapay CallService → CallRepository → FailAtHelper çağrı zincirinde orijinal hatanın StackTrace bilgisi üç metot adını da içerir.

| No | Ayrı senaryo / parametre |
|---|---|
| 74 | `—` |

### TaskAPI.API/Tests/MappingAndDomainTests.cs — MappingTests

#### EntityToDto_MapsAllResponseFields

Kaynak: [MappingTests.EntityToDto_MapsAllResponseFields](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:11)

Gerçek MappingProfile ile Id, Title, Description, CreatedAt, DueDate ve IsCompleted Entity’den DTO’ya aktarılır. DeletedAt bu DTO alanları arasında doğrulanmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 75 | `—` |

#### DtoToEntity_UsesServerCreationTimeAndIgnoresProtectedFields

Kaynak: [MappingTests.DtoToEntity_UsesServerCreationTimeAndIgnoresProtectedFields](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:28)

DTO’dan entity oluşturulurken Title/Description/DueDate aktarılır; DTO’nun Id/CreatedAt/IsCompleted değerleri kullanılmaz. Id 0, IsCompleted false, DeletedAt null ve CreatedAt güncel sunucu zamanı olur.

| No | Ayrı senaryo / parametre |
|---|---|
| 76 | `—` |

#### NullStrings_AreConvertedToEmptyStrings

Kaynak: [MappingTests.NullStrings_AreConvertedToEmptyStrings](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:48)

DTO Title ve Description null olduğunda entity’de her iki alan boş metin olur.

| No | Ayrı senaryo / parametre |
|---|---|
| 77 | `—` |

#### EntityList_IsMappedToDtoList

Kaynak: [MappingTests.EntityList_IsMappedToDtoList](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:61)

İki entity iki DTO’ya dönüştürülür; 7 ve 8 kimlikleri ve sıraları korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 78 | `—` |

### TaskAPI.API/Tests/MappingAndDomainTests.cs — UserTests

#### InvalidUsername_IsRejected

Kaynak: [UserTests.InvalidUsername_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:85)

User oluşturulurken null, boş veya yalnızca boşluk olan kullanıcı adı “Kullanıcı adı boş olamaz.” ArgumentException hatasıyla reddedilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 79 | `null` |
| 80 | `""` |
| 81 | `"   "` |

#### InvalidPasswordHash_IsRejected

Kaynak: [UserTests.InvalidPasswordHash_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/MappingAndDomainTests.cs:96)

User oluşturulurken null, boş veya yalnızca boşluk olan hash “Şifre boş olamaz.” ArgumentException hatasıyla reddedilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 82 | `null` |
| 83 | `""` |
| 84 | `"   "` |

### TaskAPI.API/Tests/RepositoryTests.cs — RepositoryTests

#### Add_PersistsTaskAndGeneratesId

Kaynak: [RepositoryTests.Add_PersistsTaskAndGeneratesId](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:38)

SQLite üzerinde görev kaydedilir, pozitif Id oluşur; takip temizlenip tekrar okunduğunda Id/Title/DueDate korunur ve DeletedAt null kalır.

| No | Ayrı senaryo / parametre |
|---|---|
| 85 | `—` |

#### Get_ExistingTask_ReturnsStoredTask

Kaynak: [RepositoryTests.Get_ExistingTask_ReturnsStoredTask](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:57)

SQLite’a eklenen görev kimliğiyle okunur; Id ve Title eşleşir.

| No | Ayrı senaryo / parametre |
|---|---|
| 86 | `—` |

#### Get_MissingTask_ReturnsNull

Kaynak: [RepositoryTests.Get_MissingTask_ReturnsNull](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:72)

SQLite’ta bulunmayan kimlik için repository null döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 87 | `—` |

#### GetAll_EmptyDatabase_ReturnsEmptyList

Kaynak: [RepositoryTests.GetAll_EmptyDatabase_ReturnsEmptyList](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:78)

SQLite görev tablosu boşken repository boş liste döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 88 | `—` |

#### GetAll_ReturnsStoredTasks

Kaynak: [RepositoryTests.GetAll_ReturnsStoredTasks](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:84)

SQLite’a kaydedilen iki görev listeye gelir; sayı ve iki başlık doğrulanır.

| No | Ayrı senaryo / parametre |
|---|---|
| 89 | `—` |

#### Update_PersistsChanges

Kaynak: [RepositoryTests.Update_PersistsChanges](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:98)

DueDate değişikliği SQLite’a kaydedilir; tekrar okuma yeni tarihi ve aynı başlığı verir.

| No | Ayrı senaryo / parametre |
|---|---|
| 90 | `—` |

#### Delete_KeepsRowAndRecordsDeletionTime

Kaynak: [RepositoryTests.Delete_KeepsRowAndRecordsDeletionTime](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:117)

Soft delete (kaydı fiziksel silmeden silinmiş işaretleme) sonrası satır durur; Id korunur, başlığa “ (Deleted)” eklenir ve DeletedAt güncel olur.

| No | Ayrı senaryo / parametre |
|---|---|
| 91 | `—` |

#### GetAll_CurrentlyIncludesSoftDeletedRows

Kaynak: [RepositoryTests.GetAll_CurrentlyIncludesSoftDeletedRows](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:139)

Mevcut sorgu davranışında silinmiş işaretlenen satır da GetAll listesinde bulunur. Test bu davranışı belgeler; gizleme/filtreleme kontrolü yapmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 92 | `—` |

#### Add_DuplicatePrimaryKey_ReportsDatabaseError

Kaynak: [RepositoryTests.Add_DuplicatePrimaryKey_ReportsDatabaseError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:155)

SQLite’a aynı Id ile ikinci kayıt eklenmesi DbUpdateException oluşturur.

| No | Ayrı senaryo / parametre |
|---|---|
| 93 | `—` |

#### AddUser_PersistsUserAndGeneratesId

Kaynak: [RepositoryTests.AddUser_PersistsUserAndGeneratesId](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:168)

SQLite’a kullanıcı kaydı eklenir; pozitif Id ve doğru username/PasswordHash tekrar okuma ile doğrulanır.

| No | Ayrı senaryo / parametre |
|---|---|
| 94 | `—` |

#### GetUserByUsername_ReturnsMatchingUser

Kaynak: [RepositoryTests.GetUserByUsername_ReturnsMatchingUser](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:184)

SQLite’tan username ile bulunan kullanıcının adı ve hash’i doğru gelir.

| No | Ayrı senaryo / parametre |
|---|---|
| 95 | `—` |

#### GetUserByUsername_MissingUser_ReturnsNull

Kaynak: [RepositoryTests.GetUserByUsername_MissingUser_ReturnsNull](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/RepositoryTests.cs:198)

SQLite’ta olmayan username için repository null döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 96 | `—` |

### TaskAPI.API/Tests/SecurityTests.cs — ConfigurationProviderTests

#### ValidBase64Key_IsDecodedCorrectly

Kaynak: [ConfigurationProviderTests.ValidBase64Key_IsDecodedCorrectly](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:13)

Bellekteki configuration’dan alınan Base64 key beklenen 32 byte değerine çözülür.

| No | Ayrı senaryo / parametre |
|---|---|
| 97 | `—` |

#### MissingOrEmptyKey_IsRejected

Kaynak: [ConfigurationProviderTests.MissingOrEmptyKey_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:31)

Key null/boş/boşluk ise “pepper key is missing or empty.” InvalidOperationException hatası oluşur.

| No | Ayrı senaryo / parametre |
|---|---|
| 98 | `null` |
| 99 | `""` |
| 100 | `"   "` |

#### InvalidBase64Key_IsRejected

Kaynak: [ConfigurationProviderTests.InvalidBase64Key_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:44)

Geçersiz Base64 key FormatException oluşturur.

| No | Ayrı senaryo / parametre |
|---|---|
| 101 | `—` |

#### RequestedConfigurationName_IsUsed

Kaynak: [ConfigurationProviderTests.RequestedConfigurationName_IsUsed](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:55)

GetKey kendisine verilen another-key ayar adını kullanır; sabit bir ayar adına bağlı olmadığı kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 102 | `—` |

### TaskAPI.API/Tests/SecurityTests.cs — PasswordHasherTests

#### HashAndVerify_CorrectPasswordSucceeds

Kaynak: [PasswordHasherTests.HashAndVerify_CorrectPasswordSucceeds](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:82)

Gerçek PasswordHasher ile hashlenen örnek şifre tekrar doğrulanır; saklanan metin düz şifreyi içermez. Latin, Türkçe ve emoji içeren örnekler denenir.

| No | Ayrı senaryo / parametre |
|---|---|
| 103 | `"Test-password-123!"` |
| 104 | `"Türkçe-Şifre-123!"` |
| 105 | `"🔐-Test-password"` |

#### WrongPassword_IsRejected

Kaynak: [PasswordHasherTests.WrongPassword_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:92)

Doğru şifrenin hash’iyle farklı şifre doğrulanamaz; Verify false döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 106 | `—` |

#### DifferentPepperKey_IsRejected

Kaynak: [PasswordHasherTests.DifferentPepperKey_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:101)

Bir pepper ile üretilen hash başka pepper kullanıldığında aynı şifreyle doğrulanamaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 107 | `—` |

#### SamePassword_ProducesDifferentSaltsAndHashes

Kaynak: [PasswordHasherTests.SamePassword_ProducesDifferentSaltsAndHashes](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:109)

Aynı şifre iki kez hashlenince salt ve saklanan hash farklı olur; her ikisi doğru şifreyle doğrulanır.

| No | Ayrı senaryo / parametre |
|---|---|
| 108 | `—` |

#### StoredFormat_ContainsExpectedVersionIterationsSaltAndHash

Kaynak: [PasswordHasherTests.StoredFormat_ContainsExpectedVersionIterationsSaltAndHash](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:123)

Saklama biçimi dört parçadır; sürüm v1, tekrar sayısı 600000, salt 16 byte ve hash 32 byte olarak doğrulanır.

| No | Ayrı senaryo / parametre |
|---|---|
| 109 | `—` |

#### UnsupportedStoredFormat_ReturnsFalse

Kaynak: [PasswordHasherTests.UnsupportedStoredFormat_ReturnsFalse](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:140)

Boş, tanınmayan, eksik parçalı veya desteklenmeyen v2 sürümlü saklama metni false verir.

| No | Ayrı senaryo / parametre |
|---|---|
| 110 | `""` |
| 111 | `"invalid"` |
| 112 | `"v1.600000.only-three-parts"` |
| 113 | `"v2.600000.salt.hash"` |

#### ModifiedHash_IsRejected

Kaynak: [PasswordHasherTests.ModifiedHash_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:146)

Saklanan hash’in bir byte’ı değiştirildiğinde doğrulama false verir.

| No | Ayrı senaryo / parametre |
|---|---|
| 114 | `—` |

#### ModifiedSalt_IsRejected

Kaynak: [PasswordHasherTests.ModifiedSalt_IsRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:159)

Saklanan salt’ın bir byte’ı değiştirildiğinde doğrulama false verir.

| No | Ayrı senaryo / parametre |
|---|---|
| 115 | `—` |

#### MalformedStoredValues_ThrowCurrentFormatException

Kaynak: [PasswordHasherTests.MalformedStoredValues_ThrowCurrentFormatException](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:175)

Tekrar sayısı sayı değilse veya salt/hash Base64 geçersizse mevcut Verify davranışı FormatException oluşturur.

| No | Ayrı senaryo / parametre |
|---|---|
| 116 | `"v1.invalid.AQ==.AQ=="` |
| 117 | `"v1.600000.not-base64!.AQ=="` |
| 118 | `"v1.600000.AQ==.not-base64!"` |

#### NonPositiveIterations_AreRejected

Kaynak: [PasswordHasherTests.NonPositiveIterations_AreRejected](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:183)

PBKDF2 tekrar sayısı 0 veya -1 olduğunda ArgumentOutOfRangeException oluşur.

| No | Ayrı senaryo / parametre |
|---|---|
| 119 | `0` |
| 120 | `-1` |

#### MissingPepperError_IsNotHidden

Kaynak: [PasswordHasherTests.MissingPepperError_IsNotHidden](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/SecurityTests.cs:192)

Key provider’ın eksik key hatası Hash çağrısında aynı mesajla dışarı çıkar.

| No | Ayrı senaryo / parametre |
|---|---|
| 121 | `—` |

### TaskAPI.API/Tests/TaskServiceTests.cs — TaskServiceTests

#### Add_InvalidTitle_ReturnsYourChosen500

Kaynak: [TaskServiceTests.Add_InvalidTitle_ReturnsYourChosen500](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:42)

Başlık null/boş/boşluk olduğunda INVALID_TASK_FORMAT/500 ve doğru mesaj kontrol edilir; entity mapping’i ve repository Add çağrılmaz. 500 mevcut bilinçli tercihtir.

| No | Ayrı senaryo / parametre |
|---|---|
| 122 | `null` |
| 123 | `""` |
| 124 | `"   "` |

#### Add_PastDate_ReturnsYourChosen500

Kaynak: [TaskServiceTests.Add_PastDate_ReturnsYourChosen500](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:53)

Geçmiş DueDate INVALID_TASK_FORMAT/500 verir; repository Add çağrılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 125 | `—` |

#### Add_ValidInput_MapsSavesAndReturnsDto

Kaynak: [TaskServiceTests.Add_ValidInput_MapsSavesAndReturnsDto](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:63)

Service DTO’yu entity’ye bir kez çevirtir, entity’yi bir kez kaydettirir, sonucu bir kez DTO’ya çevirtir ve aynı çıkış DTO’sunu döndürür. Mapper/repository mock’tur.

| No | Ayrı senaryo / parametre |
|---|---|
| 126 | `—` |

#### Add_NullMappingResult_ReportsCorrectStage

Kaynak: [TaskServiceTests.Add_NullMappingResult_ReportsCorrectStage](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:76)

Giriş mapping’i null ise domain entity mesajı ve 500 oluşur, Add yapılmaz; çıkış mapping’i null ise DTO mesajı ve 500 oluşur, Add zaten bir kez yapılmıştır. Geri alma kontrolü yoktur.

| No | Ayrı senaryo / parametre |
|---|---|
| 127 | `true` |
| 128 | `false` |

#### Get_ExistingTask_ReturnsMappedDto

Kaynak: [TaskServiceTests.Get_ExistingTask_ReturnsMappedDto](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:93)

Service Get repository’den Id 7’yi bir kez ister ve entity’yi DTO’ya bir kez dönüştürüp beklenen DTO’yu döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 129 | `—` |

#### MissingTask_Returns404AndDoesNotModifyAnything

Kaynak: [TaskServiceTests.MissingTask_Returns404AndDoesNotModifyAnything](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:106)

Get/Update/Delete için görev bulunamazsa TASK_NOT_FOUND/404 oluşur; Update/Delete ve çıkış mapping’i çağrılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 130 | `"Get"` |
| 131 | `"Update"` |
| 132 | `"Delete"` |

#### NullOutputMapping_ReturnsInternalError

Kaynak: [TaskServiceTests.NullOutputMapping_ReturnsInternalError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:128)

Get veya Update çıkış mapping’i null olduğunda INTERNAL_SERVER_ERROR/500 ve ilgili mesaj oluşur.

| No | Ayrı senaryo / parametre |
|---|---|
| 133 | `"Get"` |
| 134 | `"Update"` |

#### GetAll_ReturnsMappedList

Kaynak: [TaskServiceTests.GetAll_ReturnsMappedList](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:140)

Service GetAll repository’yi bir kez çağırır ve mapper’ın ürettiği aynı DTO listesini döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 135 | `—` |

#### GetAll_EmptyOrNullRepositoryResult_IsMappedAsEmptyList

Kaynak: [TaskServiceTests.GetAll_EmptyOrNullRepositoryResult_IsMappedAsEmptyList](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:151)

Repository boş liste veya null döndürdüğünde mapper’a boş entity listesi verilir ve service boş DTO listesi döndürür.

| No | Ayrı senaryo / parametre |
|---|---|
| 136 | `true` |
| 137 | `false` |

#### GetAll_NullMappingResult_ReturnsInternalError

Kaynak: [TaskServiceTests.GetAll_NullMappingResult_ReturnsInternalError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:167)

Liste mapping’i null olduğunda INTERNAL_SERVER_ERROR/500 ve “Tasks could not be mapped” oluşur.

| No | Ayrı senaryo / parametre |
|---|---|
| 138 | `—` |

#### Update_ChangesOnlyDueDate

Kaynak: [TaskServiceTests.Update_ChangesOnlyDueDate](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:175)

Güncellemede yalnızca DueDate değişir; Title/Description/CreatedAt/IsCompleted/DeletedAt korunur ve repository Update bir kez çağrılır.

| No | Ayrı senaryo / parametre |
|---|---|
| 139 | `—` |

#### Update_PastDate_DoesNotSave

Kaynak: [TaskServiceTests.Update_PastDate_DoesNotSave](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:201)

Geçmiş DueDate INVALID_TASK_FORMAT/500 oluşturur; eski tarih korunur ve repository Update çağrılmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 140 | `—` |

#### Delete_MarksDeletionAndCallsRepository

Kaynak: [TaskServiceTests.Delete_MarksDeletionAndCallsRepository](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:213)

Service Delete true döndürür; entity DeletedAt güncel zamanla atanır ve repository Delete bir kez çağrılır.

| No | Ayrı senaryo / parametre |
|---|---|
| 141 | `—` |

#### RepositoryFailure_IsWrappedAndOriginalErrorIsPreserved

Kaynak: [TaskServiceTests.RepositoryFailure_IsWrappedAndOriginalErrorIsPreserved](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:231)

Add/Get/GetAll/Update/Delete sırasında mock repository hatası 500 ile sarılır; mesajda işlem adı ve InnerException’da orijinal nesne korunur.

| No | Ayrı senaryo / parametre |
|---|---|
| 142 | `"Add"` |
| 143 | `"Get"` |
| 144 | `"GetAll"` |
| 145 | `"Update"` |
| 146 | `"Delete"` |

#### RepositoryCustomError_IsPreservedWithoutAnotherWrapper

Kaynak: [TaskServiceTests.RepositoryCustomError_IsPreservedWithoutAnotherWrapper](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.API/Tests/TaskServiceTests.cs:270)

Repository özel BAD_REQUEST/400 hatası service tarafından yeniden sarılmadan aynı nesne olarak aktarılır.

| No | Ayrı senaryo / parametre |
|---|---|
| 147 | `—` |

### TaskAPI.Application/Tests/ErrorHandlerTests.cs — ErrorHandlerTests

#### Handler_ReturnsExpectedError

Kaynak: [ErrorHandlerTests.Handler_ReturnsExpectedError](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.Application/Tests/ErrorHandlerTests.cs:16)

Application’daki önceki middleware testi: özel hata constructor yollarına göre belirtilen HTTP/model durumu, success false/result null, hata kodu/mesaj, geçerli GUID ve oluşturulma zamanı kontrol edilir.

| No | Ayrı senaryo / parametre |
|---|---|
| 148 | `ErrorCodes.InvalidTaskFormat, 400` |
| 149 | `ErrorCodes.TaskNotFound, 404` |
| 150 | `ErrorCodes.GenericException, 500` |
| 151 | `ErrorCodes.InternalServerError, 500` |

### TaskAPI.Application/Tests/TaskServiceTests.cs — TaskServiceTests

#### addTaskWithEmptyTitleTest

Kaynak: [TaskServiceTests.addTaskWithEmptyTitleTest](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.Application/Tests/TaskServiceTests.cs:16)

Application’daki önceki test: boş başlık için INVALID_TASK_FORMAT/400, “Title cannot be empty.” ve repository Add’ın çağrılmaması beklenir. Yeni API testinin 500 beklentisiyle çelişir.

| No | Ayrı senaryo / parametre |
|---|---|
| 152 | `—` |

#### AddTaskWithPastDueDateTest

Kaynak: [TaskServiceTests.AddTaskWithPastDueDateTest](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.Application/Tests/TaskServiceTests.cs:55)

Application’daki önceki test: geçmiş tarih için INVALID_TASK_FORMAT/400, ilgili mesaj ve repository Add’ın çağrılmaması beklenir. Yeni API testinin 500 beklentisiyle çelişir.

| No | Ayrı senaryo / parametre |
|---|---|
| 153 | `—` |

#### getTaskWithInvalidIdTest

Kaynak: [TaskServiceTests.getTaskWithInvalidIdTest](C:/Users/efebo/Desktop/TaskAPI/TaskAPI.Application/Tests/TaskServiceTests.cs:96)

Application’daki önceki test: -1 kimliği için mock repository sonuç vermediğinde TASK_NOT_FOUND/404 ve ilgili mesaj beklenir; DTO mapping’i çağrılmaz. Kimliğin negatifliğine özel ayrı bir validation kuralı kanıtlanmaz.

| No | Ayrı senaryo / parametre |
|---|---|
| 154 | `—` |

## Test yardımcıları: test değil, testlerin altyapısı

Kaynak: `TaskAPI.API/Tests/TestSupport.cs`.

| Yardımcı | Görevi |
|---|---|
| CreateAccessor | GET /api/Task/7 bilgisi taşıyan örnek HTTP context’i oluşturur. |
| CreateDto | Başlık/açıklama ve ileri tarihli örnek DTO oluşturur. |
| CreateEntity | Id 7 ve ileri tarihli örnek entity oluşturur. |
| CreateMapper | Gerçek MappingProfile’ı yükler; AssertConfigurationIsValid ile yapılandırmayı kontrol eder. Mapping testleri bu kontrolü dolaylı çalıştırır. |
| AssertTaskError | Özel exception türünü, kodunu, durumunu, iki mesaj alanını, GUID kimliğini ve atanmış zamanı kontrol eder. |
| CollectingLogger | Logları bellekte toplar; testlerin seviye, metin ve exception nesnesini incelemesini sağlar. |
| CapturedLog | Toplanan bir logun seviye/mesaj/exception bilgisini taşır. |

## Bu testlerle görünür hale gelen mevcut davranışlar

1. Task oluşturma/güncelleme kuralları service içinde çalışır. API testleri boş başlık ve geçmiş tarih için mevcut **500** tercihini kontrol eder.
2. Service görev bulamazsa özel hata ile **404** üretir. Repository bulunamayan görevde null döndürür.
3. Controller’ın mock service’den doğrudan null/false alması farklı bir senaryodur: mevcut ArgumentException kontrolleri çalışır. Gerçek service’in TASK_NOT_FOUND hatasını fırlatmasıyla bu senaryo karıştırılmamalıdır.
4. Task Update yalnızca DueDate’i değiştirir; diğer DTO alanları güncelleme izni almaz.
5. Delete fiziksel silmez: DeletedAt atanır, repository başlığa “ (Deleted)” ekler. GetAll silinmiş satırları şu an dahil eder.
6. İki parametreli özel exception/model constructor’ları varsayılan **500** kullanır. Açık statusCode verilen constructor ilgili değeri korur.
7. ExecuteAsync beklenmeyen hatayı sarar; diğer executor metotlarının testleri mevcut doğrudan tekrar fırlatma davranışını kontrol eder.
8. İç hata döngüsü 0/1/3/10/50 seviyede denenmiştir. Bu, keyfi bir sayı için matematiksel kanıt veya bütün eşzamanlı hataların toplandığı anlamına gelmez.
9. StackTrace testi yapay metot zincirinde çağrı izinin korunduğunu gösterir. Gerçek bütün mimari yollar tek tek çalıştırılmaz.
10. Giriş service’i kullanıcı yoksa/şifre yanlışsa düz Exception fırlatır. AuthController’ın 401 testi ise mock service’in false döndürmesine dayanır; ikisini gerçek HTTP üzerinden birleştiren test yoktur.
11. Hash testleri v1 biçimini, 600000 tekrar, 16 byte salt, 32 byte hash, doğru/yanlış şifre ve farklı pepper davranışlarını kontrol eder. Bir güvenlik denetiminin veya kırılmazlık kanıtının yerine geçmez.
12. Add’da çıkış mapping’i başarısız olduğunda repository kaydı daha önce gerçekleşmiş olabilir; mevcut test Add çağrısını kontrol eder, rollback (geri alma) doğrulamaz.

## Henüz test edilmeyen alanlar

Bu bölüm bir özelliğin projede kesinlikle olmadığı anlamına gelmez; okunan test dosyalarında onu doğrulayan senaryo bulunmadığını belirtir.

| Alan | Eksik kontrol | Neden önemli? |
|---|---|---|
| Gerçek HTTP akışı | Uygulama host’u ile gönderilen POST/PUT/GET/DELETE isteği, routing, middleware sırası ve cevap serialization’ı | Metotlar ayrı doğru çalışırken istek zinciri yanlış bağlanmış olabilir. |
| Model binding ve otomatik 400 | Geçersiz JSON, bozuk createdAt/dueDate, eksik gövde, gerekli DTO alanları | Scalar’da görülen .NET validation cevapları mevcut controller unit testlerinin dışında kalır. |
| Gerçek Login başarısızlığı | Gerçek AccountService yanlış şifre/kullanıcı durumundan controller/executor/handler’a kadar akış | Mock false → 401 testi gerçek Exception → cevap yolunu doğrulamaz. |
| SQL Server ve migrations | Migration’ların temiz/mevcut SQL Server’a uygulanması, eski verilerin taşınması | Repository testleri SQLite ve EnsureCreated kullanır; migration çalıştırmaz. |
| Gerçek secret yükleme | User Secrets/ortam değişkenleri, ortam seçimi, uygulama başlangıcındaki key doğrulaması | ConfigurationProvider testleri bellek içindeki örnek ayarları kullanır. |
| Eşzamanlı kayıt | Aynı username ile aynı anda Register, veritabanı unique constraint davranışı | “Önce kullanıcı var mı?” kontrolünün yarış durumları test edilmez. |
| Transaction/rollback | Kayıt sonrası mapping hatasında veya çok adımlı işte geri alma | Hata cevabı verilmesine rağmen yazılmış verinin durumu ayrıca kontrol edilmelidir. |
| Soft delete ayrıntıları | İkinci kez silme, silinen görevi Get/Update ile kullanma, filtreleme ve geri yükleme | Mevcut testler ilk silme ve GetAll’ın silinmiş satırı dahil etmesini kapsar. |
| Tarih sınırları | Tam şimdiye eşit DueDate, saat dilimi farkı, UTC/local, tarih üst sınırları | Mevcut örnekler gün farkıyla ileri/geçmiş tarih kullanır. |
| Null ve alan sınırları | DTO nesnesinin tümden null olması, çok uzun başlık/açıklama/username, şifre uzunluğu | Boş başlık ve bazı User constructor alanları dışında bütün sınırlar denenmez. |
| Hash bozulmasının tüm halleri | Null saklanan hash, aşırı büyük iteration, beklenmeyen salt/hash boyu, çok uzun/null şifre | Mevcut format testleri yalnızca belirtilen girdileri kapsar. |
| Key yaşam döngüsü | Anahtar rotasyonu, eski hash’lerin taşınması, anahtar uzunluğu politikası | Farklı pepper reddi denenir; geçiş mekanizması denenmez. |
| Cancellation | İstek iptali, CancellationToken ile DB işlemi durdurma | Async kullanılması iptal davranışının test edildiğini göstermez. |
| Response başladıktan sonra hata | Headers/body yazıldıktan sonra middleware’ın exception yakalaması | MemoryStream testlerinde hata cevabı yazımı başlamamış durum kullanılır. |
| Log hedefi ve korelasyon | Gerçek konsol/dosya hedefi, bütün katmanlarda aynı request takip numarası, logdaki tüm özel hata alanları | CollectingLogger testleri seçili kayıtları doğrular; gerçek izleme zinciri kurulmaz. |
| Performans/yük | Çok istek, büyük liste, hashleme maliyeti, zaman aşımı, bellek kullanımı | Mevcut süreler performans kriteri olarak assert edilmez. |
| Oturum/yetkilendirme | Login sonrası oturum, görev sahipliği, başka kullanıcının görevine erişim | Başarılı bool giriş testi erişim güvenliğini doğrulamaz. |
| UI | Login/register form davranışı, API adresi, tarayıcı akışı | Mevcut testler backend odaklıdır. |

## İnceleme sırasında takip edilebilecek sıra

1. Önce eski Application testlerindeki 400 ile yeni API testlerindeki 500 beklentisini tek kararla hizala.
2. Gerçek Login başarısızlığını service → controller/executor → handler birlikte test ederek son HTTP durumunu gör.
3. Geçersiz JSON/tarih için gerçek HTTP integration test (bileşenleri birlikte çalıştıran test) ekle.
4. SQL Server migration ve soft delete davranışlarını ayrıca doğrula.
5. Ardından aynı username ile eşzamanlı kayıt, rollback ve tarih sınırlarına geç.

Bu belge mevcut testleri açıklamak içindir; yukarıdaki maddeler bu incelemede uygulanmadı.

