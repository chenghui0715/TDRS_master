using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.ComponentModel;

namespace ZECS.Common.Facility
{
    public class NotDBSerializeAttribute : Attribute
    {

    }
    public class ROW_BASE
    {
        public ROW_BASE Clone()
        {
            ROW_BASE baseclass = (ROW_BASE)GetType().Assembly.CreateInstance(GetType().FullName);

            // 包含私有字段
            MemberInfo[] aryMI = this.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (MemberInfo mi in aryMI)
            {
                if (mi.MemberType != MemberTypes.Field)
                    continue;
                if (mi.DeclaringType.FullName == GetType().BaseType.FullName)
                    continue;

                try
                {
                    Object obj = this.GetType().InvokeMember(mi.Name, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, this, null);
                    if (obj != null)
                        GetType().InvokeMember(mi.Name, BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, baseclass, new Object[] { obj });
                }
                catch
                {
                    return null;
                }
            }
            return baseclass;
        }

        public static ROW_BASE Create(Type type, DataRow row, DataColumnCollection columns)
        {
            if (row == null || columns == null) return null;
            ROW_BASE baseclass = null;
            try
            {
                Assembly asm = type.Assembly;
                baseclass = (ROW_BASE)asm.CreateInstance(type.FullName);

                foreach (DataColumn column in columns)
                {
                    try
                    {
                        // Field被改为private, 须使用BindingFlags
                        MemberInfo[] mis = baseclass.GetType().GetMember(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                        if (mis == null || mis.Length == 0) continue;
                        Type mType = null;
                        foreach (MemberInfo mi in mis)
                        {

                            if (mi.MemberType == MemberTypes.Property)
                            {
                                PropertyInfo pi = (PropertyInfo)mi;
                                if (!pi.CanRead) continue;
                                mType = pi.PropertyType;
                                break;
                            }
                            else if (mi.MemberType == MemberTypes.Field)
                            {
                                FieldInfo fi = (FieldInfo)mi;
                                //if (!fi.IsPublic) continue;
                                mType = fi.FieldType;
                                break;
                            }
                            else
                                continue;
                        }
                        if (mType == null)
                            continue;

                        try
                        {
                            if ((row[column] != null) && (row[column]!=Convert.DBNull))
                            {

                                Object obj = null;
                                if (!mType.IsEnum)
                                    obj = Convert.ChangeType(row[column], mType);
                                else
                                {
                                    obj = Enum.Parse(mType, row[column].ToString());
                                }

                                baseclass.GetType().InvokeMember(column.ColumnName, BindingFlags.SetField | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic,
                                null, baseclass, new Object[] { obj/*.ToString()*/ });
                            }
                        }
                        catch(System.Exception ex)
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (System.Exception e)
            {
                return null;
            }
            return baseclass;
        }

        public static object ChangeType(object value, Type conversionType)
        {
            if (conversionType == typeof(Boolean))
            {
                string s = System.Convert.ToString(value);
                if ("false".Equals(s, StringComparison.OrdinalIgnoreCase) || s == "0" || string.IsNullOrWhiteSpace(s))
                    return false;
                else
                    return true;
            }
            else if (conversionType == typeof(DateTime))
            {
                string s = System.Convert.ToString(value);
                if (string.IsNullOrWhiteSpace(s)) return DateTime.MinValue;
                return DateTime.Parse(s);
            }
            else if (conversionType.IsEnum)
            {
                string s = System.Convert.ToString(value);
                if (string.IsNullOrWhiteSpace(s)) s = "0";
                return Enum.Parse(conversionType, s);
            }
            if (conversionType.IsGenericType &&
                    conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value != null)
                {
                    NullableConverter nullableConverter = new NullableConverter(conversionType);
                    conversionType = nullableConverter.UnderlyingType;
                }
                else
                {
                    return null;
                }
            }

            return Convert.ChangeType(value, conversionType);

        }
        public virtual Hashtable Hashtable
        {
            get
            {
                Hashtable ht = new Hashtable();

                MemberInfo[] aryMI = this.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (MemberInfo mi in aryMI)
                {
                    if (mi.MemberType != MemberTypes.Field)
                        continue;
                    
                    if (IsNotDBSerialize(mi))
                        continue;

                    Type mType = null;
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo pi = (PropertyInfo)mi;
                        if (!pi.CanRead) continue;
                        
                        mType = pi.PropertyType;
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fi = (FieldInfo)mi;
                        if (!fi.IsPublic) continue;
                        mType = fi.FieldType;
                    }
                    else
                        continue;

                    if (mType == null)
                        continue;
                    try
                    {
                        Object obj = this.GetType().InvokeMember(mi.Name, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, this, null);
                        if (obj != null)
                        {
                            if (mType.IsEnum && obj.ToString() == "NULL")
                                continue;
                            if (mType == typeof(DateTime))
                            {
                                DateTime t = (DateTime)obj;
                                //if (t.Year > 2010)
                                //{
                                ht[mi.Name] = obj;
                                //}
                            }
                            //else if (mi.Name != "COMMAND_GKEY")
                            //    ht[mi.Name] = obj;
                            else
                                ht[mi.Name] = obj;
                        }
                        //某些字段需要更新成null
                        else
                            //ht[mi.Name] = null;
                            ht[mi.Name] = Convert.DBNull;
                    }
                    catch { }
                }

                /**
                 *  更新及创建时间 可以用IsNotDBSerialize过滤
                 */
                //if (ht != null)
                //{
                //    if (ht.ContainsKey("UPDATED"))
                //        ht.Remove("UPDATED");
                //    if (ht.ContainsKey("CREATED"))
                //        ht.Remove("CREATED");
                //}

                return ht;
            }
        }
        protected bool IsNotDBSerialize(MemberInfo mi)
        {
            Object[] objAttrs = mi.GetCustomAttributes(true);
            if (objAttrs != null)
            {
                foreach (Object objAttr in objAttrs)
                {
                    if (objAttr.GetType() == typeof(NotDBSerializeAttribute))
                        return true;
                }
            }
            return false;
        }
    }
}
