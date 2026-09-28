using Tpm2Lib;

string folder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TaskAPI");
Directory.CreateDirectory(folder);

string publicPath = Path.Combine(folder, "pepper.pub");
string privatePath = Path.Combine(folder, "pepper.priv");

Tpm2Device device = new TbsDevice();
device.Connect();
var tpm = new Tpm2(device);

// 1. Ana kilit
var primaryTemplate = new TpmPublic(
    TpmAlgId.Sha256,
    ObjectAttr.Restricted | ObjectAttr.Decrypt | ObjectAttr.FixedTPM | ObjectAttr.FixedParent
        | ObjectAttr.SensitiveDataOrigin | ObjectAttr.UserWithAuth,
    null,
    new RsaParms(new SymDefObject(TpmAlgId.Aes, 128, TpmAlgId.Cfb), new NullAsymScheme(), 2048, 0),
    new Tpm2bPublicKeyRsa());

TpmHandle primary = tpm.CreatePrimary(
    TpmRh.Owner,
    new SensitiveCreate(new byte[0], null),
    primaryTemplate,
    null,
    new PcrSelection[0],
    out _, out _, out _, out _);

if (!File.Exists(privatePath))
{
    // 2. Yeni key
    byte[] key = tpm.GetRandom(32);

    // 3. Mühürleme
    var sealTemplate = new TpmPublic(
        TpmAlgId.Sha256,
        ObjectAttr.FixedTPM | ObjectAttr.FixedParent | ObjectAttr.UserWithAuth | ObjectAttr.NoDA,
        null,
        new KeyedhashParms(new NullSchemeKeyedhash()),
        new Tpm2bDigestKeyedhash());

    TpmPrivate sealedPrivate = tpm.Create(
        primary,
        new SensitiveCreate(new byte[0], key),
        sealTemplate,
        null,
        new PcrSelection[0],
        out TpmPublic sealedPublic, out _, out _, out _);

    // 4. Dosyalara kaydet
    File.WriteAllBytes(publicPath, sealedPublic.GetTpmRepresentation());
    File.WriteAllBytes(privatePath, sealedPrivate.GetTpmRepresentation());
    Console.WriteLine("Yeni key üretildi ve mühürlendi: " + folder);
}
else
{
    // 5. Mühürü açarak kontrol et
    var sealedPublic = Marshaller.FromTpmRepresentation<TpmPublic>(File.ReadAllBytes(publicPath));
    var sealedPrivate = Marshaller.FromTpmRepresentation<TpmPrivate>(File.ReadAllBytes(privatePath));

    TpmHandle sealedHandle = tpm.Load(primary, sealedPrivate, sealedPublic);
    byte[] key = tpm.Unseal(sealedHandle);
    Console.WriteLine($"Mühür açıldı, key uzunluğu: {key.Length} byte");

    tpm.FlushContext(sealedHandle);
}

tpm.FlushContext(primary);
tpm.Dispose();