using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QI_CMS_v2.Utils
{
    public static class JsonUtils
    {

        public static int GetSafeInt(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt32(); // 숫자일 경우 그대로 반환
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                if (int.TryParse(element.GetString(), out int result))
                {
                    return result; // 문자열인 경우 정수 변환
                }
            }
            return 0; // 기본값
        }

        public static double GetSafeDouble(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble(); // 숫자일 경우 그대로 반환
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                if (double.TryParse(element.GetString(), out double result))
                {
                    return result; // 문자열인 경우 실수 변환
                }
            }
            return 0; // 기본값
        }


    }
}
