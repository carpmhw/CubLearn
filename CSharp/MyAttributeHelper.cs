using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreMVC.Helpers
{
    public static class MyAttributesHelper
    {

        /// <summary>
        /// https://stackoverflow.com/questions/5015830/get-the-value-of-displayname-attribute
        /// string displayName = ReflectionExtensions.GetPropertyDisplayName<SomeClass>(i => i.SomeProperty);
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo member, bool isRequired) where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The {0} attribute must be defined on member {1}",
                        typeof(T).Name,
                        member.Name));
            }

            return (T)attribute;
        }

        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            MemberExpression memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }

        /// <summary>
        /// Retrieve the DisplayName attribute out of a property
        /// https://dotblogs.com.tw/johnny/2015/07/31/csharp-custom-attributes
        /// </summary>
        /// <typeparam name="T">The class</typeparam>
        /// <param name="propertyName">The property name</param>
        public static string GetDisplayName<T>(string propertyName)
        {
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var attr = (DisplayNameAttribute[])property.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (0 < attr.Length)
                return attr[0].DisplayName;
            else
                throw new NullReferenceException("Johnny.GetDisplayName() error: Attempt to retrieve the non-existent \"DisplayNameAttribute\" attribute out of property \"" + propertyName + "\" in class " + typeof(T) + "! You must define it before using it.");
        }
    }
}