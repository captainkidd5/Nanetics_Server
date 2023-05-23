using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{
    /// <summary>
    ///https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-10-31-previewdataplane/devices/get-credentials?tabs=HTTP#x509certificateinfo
    /// </summary>
    public class DeviceCredentials
    {
        public string IdScope { get; set; }
        public SymmetricKey SymmetricKey { get; set; }
        public Tpm Tpm { get; set; }
        public X509Certificate X509Certificate { get; set; }
    }

    public class SymmetricKey
    {
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }

    public class Tpm
    {
        public string EndorsementKey { get; set; }
    }


    public class X509Certificate
    {
        public string Certificate { get; set; }
        public X509CertificateInfo Info { get; set; }
    }
    public class X509CertificateInfo
    {
        public string Sha1Thumbprint { get; set; }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-07-31dataplane/enrollment-groups/create-x509?tabs=HTTP#certificateentry
    /// </summary>
    public class CertificateEntry
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }

    public class CreateEnrollmentGroupX509Request
    {
        public string Certificate { get; set; }
        public string ETag { get; set; }
        public bool Verified { get; set; }
    }
}
