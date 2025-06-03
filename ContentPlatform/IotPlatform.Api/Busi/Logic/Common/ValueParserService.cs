using IotPlatform.Api.Entities;

namespace IotPlatform.Api.Busi.Logic.Common;

public class ValueParserService(ILogger<ValueParserService> logger)
{
    public TypeCode Str2TypeCode(string datatype)
    {
        var accode = "";
        //typecode=Type.GetTypeCode(item.Value.GetType())
        foreach (int tCode in Enum.GetValues(typeof(TypeCode)))
        {
            if (((TypeCode)tCode).ToString().ToUpper() == datatype.ToUpper())
            {
                accode = ((TypeCode)tCode).ToString();
            }
            else if (datatype.ToUpper() == "BOOL")
            {
                accode = "Boolean";
            }
            else if (datatype.ToUpper() == "INT")
            {
                accode = "Int32";
            }
            else if (datatype.ToUpper() == "LONG")
            {
                accode = "Int64";
            }
        }

        if (accode == "")
        {
            throw new Exception("不存在  {datatype}  的类型");
        }

        var typecode = (TypeCode)Enum.Parse(typeof(TypeCode), accode, true);
        return typecode;
    }

    public List<TypeCode> GetTypeCodes()
    {
        return new TypeCode[]
        {
            Str2TypeCode("Boolean"),
            Str2TypeCode("String"),
            Str2TypeCode("Double"),
            Str2TypeCode("Single"),
            Str2TypeCode("Decimal"),
            Str2TypeCode("Bool"),
            Str2TypeCode("Char"),
            Str2TypeCode("Byte"),
            Str2TypeCode("Single"),
            Str2TypeCode("Int16"),
            Str2TypeCode("UInt16"),
            Str2TypeCode("Int32"),
            Str2TypeCode("UInt32"),
            Str2TypeCode("Int64"),
            Str2TypeCode("UInt64"),
            Str2TypeCode("DateTime"),
        }.ToList();
    }

    public void ConvertType(string value, TagEntity tagInfo)
    {
        //string str, uint uint32,bool boolean,short int16,ushort uint16,int int32 ,long lo,ulong lo64,float fl,double db)> { }
        var tagtmp = tagInfo;
        var item = tagInfo;
        if (tagtmp == null)
        {
            //不存在该配置
            logger.LogWarning($"不存在  {tagInfo.TagCode}  的配置");
            tagInfo.Value = new ObjValue(){Str = value};
        }


        var datatype = tagtmp.DataType;
        TypeCode typecode = TypeCode.DBNull;
        try
        {
            typecode = Str2TypeCode(datatype);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, $"{tagInfo.TagCode}  " + e.Message);
        }

        switch (typecode)
        {
            case TypeCode.DateTime:
            {
                var newtag = Convert.ToDateTime(value);
                tagInfo.Value = new ObjValue() { Str = newtag.ToString("yyyy-MM-dd HH:mm:ss") };
            }
                break;

            case TypeCode.Boolean:
            {
                var newtag = Convert.ToBoolean(value);
                tagInfo.Value = new ObjValue(){Boolean = (newtag)};
            }
                break;
            case TypeCode.Char:
            {
                var newtag = Convert.ToChar(value);
                tagInfo.Value = new ObjValue(){Uint16 = (newtag)};
            }
                break;

            case TypeCode.Byte:
            {
                var newtag = (byte)ConvertNumber(value, typecode, tagtmp);
                tagInfo.Value = new ObjValue(){Byte = newtag};
            }
                break;
            case TypeCode.Int16:
            {
                var convertNumber = (short)ConvertNumber(value, typecode, tagtmp);
                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Int16 = (newtag)};
            }
                break;
            case TypeCode.UInt16:
            {
                var convertNumber = (ushort)ConvertNumber(value, typecode, tagtmp);

                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Uint16 = (newtag)};
            }
                break;
            case TypeCode.Int32:
            {
                var convertNumber = (int)ConvertNumber(value, typecode, tagtmp);


                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Int32 = (newtag)};
            }
                break;
            case TypeCode.UInt32:
            {
                var convertNumber = (uint)ConvertNumber(value, typecode, tagtmp);
                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Uint32 = (newtag)};
            }
                break;
            case TypeCode.Int64:
            {
                var convertNumber = (long)ConvertNumber(value, typecode, tagtmp);


                var newtag = convertNumber;

                tagInfo.Value = new ObjValue(){Long = (newtag)};
            }
                break;
            case TypeCode.UInt64:
            {
                var convertNumber = (ulong)ConvertNumber(value, typecode, tagtmp);


                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Ulong = (newtag)};
            }
                break;
            case TypeCode.Single:
            {
                var convertNumber = (float)ConvertNumber(value, typecode, tagtmp);


                var newtag = convertNumber;
                if (convertNumber.ToString() == "NaN")
                {
                    newtag = 0;
                }

                tagInfo.Value = new ObjValue(){Float = (newtag)};
            }
                break;
            case TypeCode.Double:
            {
                var convertNumber = (double)ConvertNumber(value, typecode, tagtmp);
                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Double = (newtag)};
            }
                break;
            case TypeCode.Decimal:
            {
                var convertNumber = (decimal)ConvertNumber(value, typecode, tagtmp);


                var newtag = convertNumber;
                tagInfo.Value = new ObjValue(){Decimal = (newtag)};
            }
                break;
            case TypeCode.String:
            {
                var newtag = Convert.ToString(value);
                tagInfo.Value = new ObjValue(){Str = newtag};
            }
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private object ConvertNumber(string value, TypeCode destType, TagEntity tagInfo)
    {
        //没有计算必要，则直接返回
        if (tagInfo.Scaling == 1F && tagInfo.Shifting == 0F) return Convert.ChangeType(value, destType);

        //decimal直接运算并返回
        if (destType == TypeCode.Decimal)
        {
            return Convert.ToDecimal(value) *( tagInfo.Scaling ==null?(decimal)1: (decimal)tagInfo.Scaling )+ ((decimal)(tagInfo.Shifting==null?0:tagInfo.Shifting));
        }

        //其余数据类型以double运算
        var s = Convert.ToDouble(value) * ( tagInfo.Scaling ==null?(double)1: (double)tagInfo.Scaling ) +  ((double)(tagInfo.Shifting==null?0:tagInfo.Shifting));

        object result;
        try
        {
            //使用Convert来实现四舍五入和数据类型转换，以及抛出数据溢出异常
            result =  Convert.ChangeType(s, destType);
        }
        catch (OverflowException)
        {
            //整数数据类型产生了溢出，返回该类型最大或最小值
            switch (destType)
            {
                case TypeCode.Byte:
                    return s > byte.MaxValue ? byte.MaxValue : byte.MinValue;
                case TypeCode.Int16:
                    return s > short.MaxValue ? short.MaxValue : short.MinValue;
                case TypeCode.Int32:
                    return s > int.MaxValue ? int.MaxValue : int.MinValue;
                case TypeCode.Int64:
                    return s > long.MaxValue ? long.MaxValue : long.MinValue;
                case TypeCode.SByte:
                    return s > sbyte.MaxValue ? sbyte.MaxValue : sbyte.MinValue;
                case TypeCode.UInt16:
                    return s > ushort.MaxValue ? ushort.MaxValue : ushort.MinValue;
                case TypeCode.UInt32:
                    return s > uint.MaxValue ? uint.MaxValue : uint.MinValue;
                case TypeCode.UInt64:
                    return s > ulong.MaxValue ? ulong.MaxValue : ulong.MinValue;
                default:
                    throw;
            }
        }

        //浮点数非数值或无限值，返回0或最大值或最小值
        switch (result)
        {
            case float f when float.IsNaN(f):
                return 0F;
            case float f when float.IsPositiveInfinity(f):
                return float.MaxValue;
            case float f when float.IsNegativeInfinity(f):
                return float.MaxValue;
            case double d when double.IsNaN(d):
                return 0D;
            case double d when double.IsPositiveInfinity(d):
                return double.MaxValue;
            case double d when double.IsNegativeInfinity(d):
                return double.MinValue;
            default:
                return result;
        }
    }
}