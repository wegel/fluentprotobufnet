using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FluentProtobufNet.Mapping;

namespace FluentProtobufNet.Helpers
{
    public static class ReflectionExtensions
    {
        public static Member ToMember<TMapping, TReturn>(this Expression<Func<TMapping, TReturn>> propertyExpression)
        {
            return ReflectionHelper.GetMember(propertyExpression);
        }

        public static class ReflectionHelper
        {
            /*public static Type AutomappingTypeForEntityType(Type entityType)
            {
                return typeof(AutoMapping<>).MakeGenericType(entityType);
            }*/

            public static Member GetMember<TModel, TReturn>(Expression<Func<TModel, TReturn>> expression)
            {
                return GetMember(expression.Body);
            }

            public static Member GetMember<TModel>(Expression<Func<TModel, object>> expression)
            {
                return GetMember(expression.Body);
            }

            public static Accessor GetAccessor<MODEL>(Expression<Func<MODEL, object>> expression)
            {
                MemberExpression memberExpression = GetMemberExpression(expression.Body);

                return getAccessor(memberExpression);
            }

            public static Accessor GetAccessor<MODEL, T>(Expression<Func<MODEL, T>> expression)
            {
                MemberExpression memberExpression = GetMemberExpression(expression.Body);

                return getAccessor(memberExpression);
            }

            private static bool IsIndexedPropertyAccess(Expression expression)
            {
                return IsMethodExpression(expression) && expression.ToString().Contains("get_Item");
            }

            private static bool IsMethodExpression(Expression expression)
            {
                return expression is MethodCallExpression || (expression is UnaryExpression && IsMethodExpression((expression as UnaryExpression).Operand));
            }

            private static Member GetMember(Expression expression)
            {
                if (IsIndexedPropertyAccess(expression))
                    return GetDynamicComponentProperty(expression).ToMember();
                if (IsMethodExpression(expression))
                    return ((MethodCallExpression)expression).Method.ToMember();

                var memberExpression = GetMemberExpression(expression);

                return memberExpression.Member.ToMember();
            }

            private static PropertyInfo GetDynamicComponentProperty(Expression expression)
            {
                Type desiredConversionType = null;
                MethodCallExpression methodCallExpression = null;
                var nextOperand = expression;

                while (nextOperand != null)
                {
                    if (nextOperand.NodeType == ExpressionType.Call)
                    {
                        methodCallExpression = nextOperand as MethodCallExpression;
                        desiredConversionType = desiredConversionType ?? methodCallExpression.Method.ReturnType;
                        break;
                    }

                    if (nextOperand.NodeType != ExpressionType.Convert)
                        throw new ArgumentException("Expression not supported", "expression");

                    var unaryExpression = (UnaryExpression)nextOperand;
                    desiredConversionType = unaryExpression.Type;
                    nextOperand = unaryExpression.Operand;
                }

                var constExpression = methodCallExpression.Arguments[0] as ConstantExpression;

                return new DummyPropertyInfo((string)constExpression.Value, desiredConversionType);
            }

            private static MemberExpression GetMemberExpression(Expression expression)
            {
                return GetMemberExpression(expression, true);
            }

            private static MemberExpression GetMemberExpression(Expression expression, bool enforceCheck)
            {
                MemberExpression memberExpression = null;
                if (expression.NodeType == ExpressionType.Convert)
                {
                    var body = (UnaryExpression)expression;
                    memberExpression = body.Operand as MemberExpression;
                }
                else if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpression = expression as MemberExpression;
                }

                if (enforceCheck && memberExpression == null)
                {
                    throw new ArgumentException("Not a member access", "expression");
                }

                return memberExpression;
            }

            private static Accessor getAccessor(MemberExpression memberExpression)
            {
                var list = new List<Member>();

                while (memberExpression != null)
                {
                    list.Add(memberExpression.Member.ToMember());
                    memberExpression = memberExpression.Expression as MemberExpression;
                }

                if (list.Count == 1)
                {
                    return new SingleMember(list[0]);
                }

                list.Reverse();
                return new PropertyChain(list.ToArray());
            }
        }

        public class SingleMember : Accessor
        {
            private readonly Member member;

            public SingleMember(Member member)
            {
                this.member = member;
            }

            #region Accessor Members

            public string FieldName
            {
                get { return member.Name; }
            }

            public Type PropertyType
            {
                get { return member.PropertyType; }
            }

            public Member InnerMember
            {
                get { return member; }
            }

            public Accessor GetChildAccessor<T>(Expression<Func<T, object>> expression)
            {
                var property = expression.ToMember();
                return new PropertyChain(new[] { member, property });
            }

            public string Name
            {
                get { return member.Name; }
            }

            public void SetValue(object target, object propertyValue)
            {
                member.SetValue(target, propertyValue);
            }

            public object GetValue(object target)
            {
                return member.GetValue(target);
            }

            #endregion

            public static SingleMember Build<T>(Expression<Func<T, object>> expression)
            {
                var member = expression.ToMember();
                return new SingleMember(member);
            }

            public static SingleMember Build<T>(string propertyName)
            {
                var member = typeof(T).GetProperty(propertyName).ToMember();
                return new SingleMember(member);
            }
        }

        public interface Accessor
        {
            string FieldName { get; }

            Type PropertyType { get; }
            Member InnerMember { get; }
            void SetValue(object target, object propertyValue);
            object GetValue(object target);

            Accessor GetChildAccessor<T>(Expression<Func<T, object>> expression);

            string Name { get; }
        }

        public class PropertyChain : Accessor
        {
            private readonly Member[] _chain;
            private readonly SingleMember innerMember;

            public PropertyChain(Member[] members)
            {
                _chain = new Member[members.Length - 1];
                for (int i = 0; i < _chain.Length; i++)
                {
                    _chain[i] = members[i];
                }

                innerMember = new SingleMember(members[members.Length - 1]);
            }

            #region Accessor Members

            public void SetValue(object target, object propertyValue)
            {
                target = findInnerMostTarget(target);
                if (target == null)
                {
                    return;
                }

                innerMember.SetValue(target, propertyValue);
            }

            public object GetValue(object target)
            {
                target = findInnerMostTarget(target);

                if (target == null)
                {
                    return null;
                }

                return innerMember.GetValue(target);
            }

            public string FieldName
            {
                get { return innerMember.FieldName; }
            }

            public Type PropertyType
            {
                get { return innerMember.PropertyType; }
            }

            public Member InnerMember
            {
                get { return innerMember.InnerMember; }
            }

            public Accessor GetChildAccessor<T>(Expression<Func<T, object>> expression)
            {
                var member = expression.ToMember();
                var list = new List<Member>(_chain);
                list.Add(innerMember.InnerMember);
                list.Add(member);

                return new PropertyChain(list.ToArray());
            }

            public string Name
            {
                get
                {
                    string returnValue = string.Empty;
                    foreach (var info in _chain)
                    {
                        returnValue += info.Name + ".";
                    }

                    returnValue += innerMember.Name;

                    return returnValue;
                }
            }

            #endregion

            private object findInnerMostTarget(object target)
            {
                foreach (var info in _chain)
                {
                    target = info.GetValue(target);
                    if (target == null)
                    {
                        return null;
                    }
                }

                return target;
            }
        }

    }
}
