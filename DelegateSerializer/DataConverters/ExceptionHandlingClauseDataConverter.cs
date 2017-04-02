using System;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Data;
using DelegateSerializer.Exceptions;

namespace DelegateSerializer.DataConverters
{
    internal class ExceptionHandlingClauseDataConverter
    {
        private readonly TypeInfoDataConverter typeInfoDataConverter;
        private readonly TypeResolver typeResolver;

        public ExceptionHandlingClauseDataConverter(TypeInfoDataConverter typeInfoDataConverter,
                                                  TypeResolver typeResolver)
        {
            this.typeInfoDataConverter = typeInfoDataConverter;
            this.typeResolver = typeResolver;
        }

        public ExceptionHandlingClauseData Build(ExceptionHandlingClause c)
        {
            return new ExceptionHandlingClauseData
                   {
                       Flags = c.Flags,
                       CatchType =
                           c.Flags == ExceptionHandlingClauseOptions.Clause
                               ? typeInfoDataConverter.Build(c.CatchType)
                               : null,
                       TryOffset = c.TryOffset,
                       TryLength = c.TryLength,
                       HandlerOffset = c.HandlerOffset,
                       HandlerLength = c.HandlerLength
                   };
        }

        public void Emit(ILGenerator il, int offset, ExceptionHandlingClauseData clause)
        {
            if (offset == clause.TryOffset)
                il.BeginExceptionBlock();
            if (offset == clause.HandlerOffset)
            {
                switch (clause.Flags)
                {
                    case ExceptionHandlingClauseOptions.Finally:
                        il.BeginFinallyBlock();
                        break;
                    case ExceptionHandlingClauseOptions.Fault:
                        il.BeginFaultBlock();
                        break;
                    case ExceptionHandlingClauseOptions.Clause:
                        il.BeginCatchBlock(typeResolver.GetType(clause.CatchType));
                        break;
                    default:
                        throw new DelegateDeserializationException(string.Format("Unknown clause {0}", clause.Flags));
                }
            }
            if (offset == clause.HandlerOffset + clause.HandlerLength)
                il.EndExceptionBlock();
        }
    }
}