using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastMember
{
    /// <summary>
    /// Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IEnumerable<Member>, IList<Member>
    {
        Member[] members;
        internal MemberSet(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            members = type.GetTypeAndInterfaceProperties(PublicInstance).Cast<MemberInfo>().Concat(type.GetFields(PublicInstance).Cast<MemberInfo>()).OrderBy(x => x.Name)
                .Select(member => new Member(member)).ToArray();
        }
        /// <summary>
        /// Return a sequence of all defined members
        /// </summary>
        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in members) yield return member;
        }
        /// <summary>
        /// Get a member by index
        /// </summary>
        public Member this[int index]
        {
            get { return members[index]; }
        }
        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        public int Count { get { return members.Length; } }
        Member IList<Member>.this[int index]
        {
            get { return members[index]; }
            set { throw new NotSupportedException(); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
        bool ICollection<Member>.Remove(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Add(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Clear() { throw new NotSupportedException(); }
        void IList<Member>.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList<Member>.Insert(int index, Member item) { throw new NotSupportedException(); }

        bool ICollection<Member>.Contains(Member item)  => members.Contains(item);
        void ICollection<Member>.CopyTo(Member[] array, int arrayIndex) { members.CopyTo(array, arrayIndex); }
        bool ICollection<Member>.IsReadOnly { get { return true; } }
        int IList<Member>.IndexOf(Member member) { return Array.IndexOf<Member>(members, member); }
        
    }
    /// <summary>
    /// Represents an abstracted view of an individual member defined for a type
    /// </summary>
    public sealed class Member
    {
        public MemberInfo Info { get; }
        
        internal Member(MemberInfo member)
        {
            this.Info = member;
        }
        /// <summary>
        /// The ordinal of this member among other members.
        /// Returns -1 in case the ordinal is not set.
        /// </summary>
        public int Ordinal
        {
            get
            {
                var ordinalAttr = Info.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(OrdinalAttribute));

                if (ordinalAttr == null)
                {
                    return -1;
                }

                // OrdinalAttribute class must have only one constructor with a single argument.
                return Convert.ToInt32(ordinalAttr.ConstructorArguments.Single().Value);
            }
        }
        /// <summary>
        /// The name of this member
        /// </summary>
        public string Name { get { return Info.Name; } }
        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        public Type Type
        {
            get
            {
                if(Info is FieldInfo) return ((FieldInfo)Info).FieldType;
                if (Info is PropertyInfo) return ((PropertyInfo)Info).PropertyType;
                throw new NotSupportedException(Info.GetType().Name);
            }
        }

        public bool IsPublic 
        {
            get {
                if(Info is FieldInfo) return ((FieldInfo)Info).IsPublic;
                if (Info is PropertyInfo) return ((PropertyInfo)Info).GetSetMethod().IsPublic;
                throw new NotSupportedException(Info.GetType().Name);
            }
        }

        public bool IsField => Info is FieldInfo;

        public bool IsProperty => Info is PropertyInfo;

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            return Attribute.IsDefined(Info, attributeType);
        }

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute(Type attributeType, bool inherit)
            => Attribute.GetCustomAttribute(Info, attributeType, inherit);

		/// <summary>
		/// Property Can Write
		/// </summary>
		public bool CanWrite
        {
            get
            {
                switch (Info.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)Info).CanWrite;
                    default: throw new NotSupportedException(Info.MemberType.ToString());
                }
            }
        }

        /// <summary>
        /// Property Can Read
        /// </summary>
        public bool CanRead
        {
            get
            {
                switch (Info.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)Info).CanRead;
                    default: throw new NotSupportedException(Info.MemberType.ToString());
                }
            }
        }
    }
}
