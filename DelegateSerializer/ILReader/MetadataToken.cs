using System;
using System.Linq;
using System.Reflection;

namespace DelegateSerializer.ILReader
{
    internal class MetadataToken
    {
        public MetadataToken(uint token)
        {
            this.token = token;
        }

        public MetadataToken(TokenType type)
            : this(type, 0)
        {
        }

        public MetadataToken(TokenType type, uint rid)
        {
            token = (uint) type | rid;
        }

        public MetadataToken(TokenType type, int rid)
        {
            token = (uint) type | (uint) rid;
        }

        public uint RID
        {
            get { return token & 0x00ffffff; }
        }

        public TokenType TokenType
        {
            get { return (TokenType) (token & 0xff000000); }
        }

        public int ToInt32()
        {
            return (int) token;
        }

        public uint ToUInt32()
        {
            return token;
        }

        public override string ToString()
        {
            return string.Format("[{0}:0x{1}]", TokenType, RID.ToString("x4"));
        }

        readonly uint token;

        public static readonly MetadataToken Zero = new MetadataToken((uint) 0);

        private static readonly Type __Canon = typeof(object).Assembly.GetType("System.__Canon");
        private static readonly Type[] universalArguments = Enumerable.Repeat(__Canon, 1024).ToArray();

        public object Resolve(Module module)
        {
            switch (TokenType)
            {
                case TokenType.Method:
                case TokenType.MethodSpec:
                    return module.ResolveMethod(ToInt32(), universalArguments, universalArguments);
                case TokenType.MemberRef:
                    return module.ResolveMember(ToInt32(), universalArguments, universalArguments);
                case TokenType.Field:
                    return module.ResolveField(ToInt32(), universalArguments, universalArguments);
                case TokenType.TypeDef:
                case TokenType.TypeRef:
                    return module.ResolveType(ToInt32(), universalArguments, universalArguments);
                case TokenType.Signature:
                    return module.ResolveSignature(ToInt32());
                case TokenType.String:
                    return module.ResolveString(ToInt32());
                default:
                    throw new NotSupportedException();
            }
        }
    }
}