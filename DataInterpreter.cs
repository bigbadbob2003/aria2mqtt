using System.Globalization;
namespace Aria2Mqtt;

public class DataReading
{
    public uint UserId { get; set; }
    public double WeightKg { get; set; }
    public double WeightSt { get; set; }
    public double WeightLbs { get; set; }
    public uint Date { get; set; }
    public string FriendlyDate { get; set; }
    public uint Impedance { get; set; }
    public uint BodyFat1 { get; set; }
    public uint BodyFat2 { get; set; }
    public uint Covariance { get; set; }
}

public class DataInterpretation
{
    public uint Date { get; set; }
    public string FriendlyDate { get; set; }
    public uint ItemCount { get; set; }
    public uint Battery { get; set; }
    public uint Protocol { get; set; }
    public uint Firmware { get; set; }
    public string Mac { get; set; }
    public List<DataReading> Readings { get; set; }
}

public class DataInterpreter
{
    public static DataInterpretation InterpretData(byte[] postData, bool ignoreRegisteredUsers = false)
    {
        var header = postData.Take(46).ToArray();
        var buffer = postData.Skip(46).ToArray();
        var itemStrings = new List<byte[]>();

        while (buffer.Length >= 32)
        {
            itemStrings.Add(buffer.Take(32).ToArray());
            buffer = buffer.Skip(32).ToArray();
        }

        var checksum = buffer;

        var ret = new DataInterpretation
        {
            Date = BitConverter.ToUInt32(header, 38),
            FriendlyDate = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(header, 38))
                .ToString("dddd d MMMM yyyy, h:mmtt", CultureInfo.InvariantCulture) + " UTC",
            ItemCount = BitConverter.ToUInt32(header, 42),
            Battery = BitConverter.ToUInt32(header, 4),
            Protocol = BitConverter.ToUInt32(header, 0),
            Firmware = BitConverter.ToUInt32(header, 30),
            Mac = string.Join(":", header.Skip(8).Take(6).Select(b => b.ToString("X2"))),
            Readings = new List<DataReading>()
        };

        foreach (var chunk in itemStrings)
        {
            var item = new DataReading
            {
                UserId = BitConverter.ToUInt32(chunk, 16)
            };

            if (item.UserId > 0 && ignoreRegisteredUsers)
            {
                continue;
            }

            item.WeightKg = BitConverter.ToUInt32(chunk, 8) / 1000.0;
            item.WeightSt = BitConverter.ToUInt32(chunk, 8) / 6350.293;
            item.WeightLbs = BitConverter.ToUInt32(chunk, 8) / 453.6;
            item.Date = BitConverter.ToUInt32(chunk, 12);
            item.FriendlyDate = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(chunk, 12))
                .ToString("dddd d MMMM yyyy, h:mmtt", CultureInfo.InvariantCulture) + " UTC";
            item.Impedance = BitConverter.ToUInt32(chunk, 4);
            item.BodyFat1 = BitConverter.ToUInt32(chunk, 20);
            item.BodyFat2 = BitConverter.ToUInt32(chunk, 28);
            item.Covariance = BitConverter.ToUInt32(chunk, 24);

            ret.Readings.Add(item);
        }

        return ret;
    }
}
