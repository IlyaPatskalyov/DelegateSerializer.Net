using System;
using System.Reflection;
using System.Reflection.Emit;
using DelegateSerializer.Data;

namespace DelegateSerializer.DataBuilders
{
    public class ExceptionHandlingClauseDataBuilder
    {
        private readonly TypeInfoDataBuilder typeInfoDataBuilder;
        private readonly TypeResolver typeResolver;

        public ExceptionHandlingClauseDataBuilder(TypeInfoDataBuilder typeInfoDataBuilder,
                                                  TypeResolver typeResolver)
        {
            this.typeInfoDataBuilder = typeInfoDataBuilder;
            this.typeResolver = typeResolver;
        }

        public ExceptionHandlingClauseData Build(ExceptionHandlingClause c)
        {
            return new ExceptionHandlingClauseData
                   {
                       Flags = c.Flags,
                       CatchType =
                           c.Flags == ExceptionHandlingClauseOptions.Clause
                               ? typeInfoDataBuilder.Build(c.CatchType)
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
                        throw new Exception(string.Format("Unknown clause {0}", clause.Flags));
                }
            }
            if (offset == clause.HandlerOffset + clause.HandlerLength)
                il.EndExceptionBlock();
        }
    }
}