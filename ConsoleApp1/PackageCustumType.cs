using ServForOracle.NetCore.OracleAbstracts;

namespace ConsoleApp1;

[OracleUdt(schame, type, typeTable)]
public class PackageCustumType
{
    [OracleUdtProperty("ACODE")]
    public string Code { get; set; } = "1";

    [OracleUdtProperty("ATYPE")]
    public int Type { get; set; } = 1;
}

public class Constants
{
    public const string conString = "USER ID=trd;Password=trd1234;DATA SOURCE=10.26.7.80:1521/trdkrx;";
    public const string producerName = "TRD.SPTRD_INSERT_PACKAGE";
    public const string typeTable = "RTRD_PACKAGE_TABLE";
    public const string type = "RTRD_PACKAGE_TYPE";
    public const string schame = "TRD";
    public const string PDATA = "PDATA";
}