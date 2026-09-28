using Tpm2Lib;
using TaskAPI.Entities.Interfaces;

namespace TaskAPI.Infrastructure.Security
{
    public class TpmPepperKeyProvider : IPepperKeyProvider
    {
        private readonly byte[] _key = UnsealKey();

        public byte[] GetKey()
        {
            return _key;
        }

        private static byte[] UnsealKey()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TaskAPI");
            string publicPath = Path.Combine(folder, "pepper.pub");
            string privatePath = Path.Combine(folder, "pepper.priv");

            if (!File.Exists(publicPath) || !File.Exists(privatePath))
            {
                throw new InvalidOperationException($"TPM mühürlü key dosyaları bulunamadı: {folder}");
            }

            Tpm2Device device = new TbsDevice();
            device.Connect();
            using var tpm = new Tpm2(device);

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

            var sealedPublic = Marshaller.FromTpmRepresentation<TpmPublic>(File.ReadAllBytes(publicPath));
            var sealedPrivate = Marshaller.FromTpmRepresentation<TpmPrivate>(File.ReadAllBytes(privatePath));

            TpmHandle sealedHandle = tpm.Load(primary, sealedPrivate, sealedPublic);
            byte[] key = tpm.Unseal(sealedHandle);

            tpm.FlushContext(sealedHandle);
            tpm.FlushContext(primary);

            return key;
        }
    }
}