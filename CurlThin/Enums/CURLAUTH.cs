namespace CurlThin.Enums
{
    public enum CURLAUTH : int
    {
        NONE = 0,
        BASIC = (1 << 0),             // HTTP Basic authentication (default)
        DIGEST = (1 << 1),            // HTTP Digest authentication
        NEGOTIATE = (1 << 2),         // HTTP Negotiate (SPNEGO) authentication
        NTLM = (1 << 3),              // HTTP NTLM authentication
        DIGEST_IE = (1 << 4),         // HTTP Digest authentication with IE flavour
        NTLM_WB = (1 << 5),           // HTTP NTLM authentication delegated to winbind helper
        BEARER = (1 << 6),            // HTTP Bearer token authentication
        AWS_SIGV4 = (1 << 7),         // HTTP AWS V4 signature authentication

        ONLY = (1 << 31),             // Use together with a single other type to force no authentication or just that single type
        ANY = ~0,                     // All types set
        ANYSAFE = (~(CURLAUTH.BASIC|DIGEST_IE))
    }
}