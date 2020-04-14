/********************************************************
 * Chinese Name: 赵可(Zhao Ke)
 * English Name: Kathy Zhao
 * CreateTime: April 1, 2018 
 * Email:kathyatc@outlook.com
 * 
 * Copyright © 2018 Kathy Zhao. All Rights Reserved.
 * *******************************************************/
using System;

namespace Monitor
{
    public enum SpeedUnitType
    {
        [StringValue("UB")]
        BPS,
        [StringValue("KB")]
        KBPS,
        [StringValue("MB")]
        MBPS,
        [StringValue("GB")]
        GBPS,
        [StringValue("TB")]
        TBPS,
    }

    public class StringValue : System.Attribute
    {
        private string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public static class EnumExpand
    {
        public static string GetStringValue(this Enum value)
        {
            string output = null;
            System.Type type = value.GetType();
            System.Reflection.FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs = fi.GetCustomAttributes(typeof(StringValue), false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }
    }
}