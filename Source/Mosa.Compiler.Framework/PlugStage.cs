/*
 * (c) 2012 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Collections.Generic;

using Mosa.Compiler.Common;
using Mosa.Compiler.Linker;
using Mosa.Compiler.TypeSystem;
using Mosa.Compiler.Metadata.Signatures;

namespace Mosa.Compiler.Framework
{
	/// <summary>
	/// Emits metadata for assemblies and types
	/// </summary>
	public class PlugStage : BaseAssemblyCompilerStage, IAssemblyCompilerStage
	{
		#region Data members

		private IAssemblyLinker linker;

		// Method to Plug -> Plug
		protected Dictionary<RuntimeMethod, RuntimeMethod> plugMethods = new Dictionary<RuntimeMethod, RuntimeMethod>();

		protected RuntimeType plugTypeAttribute;
		protected RuntimeType plugMethodAttribute;

		#endregion // Data members

		#region IAssemblyCompilerStage members

		void IAssemblyCompilerStage.Setup(AssemblyCompiler compiler)
		{
			base.Setup(compiler);
			this.linker = RetrieveAssemblyLinkerFromCompiler();

			plugTypeAttribute = typeSystem.GetType("Mosa.Internal.Plug", "Mosa.Internal.Plug", "PlugTypeAttribute");
			plugMethodAttribute = typeSystem.GetType("Mosa.Internal.Plug", "Mosa.Internal.Plug", "PlugMethodAttribute");
		}

		void IAssemblyCompilerStage.Run()
		{
			if (plugTypeAttribute == null | plugMethodAttribute == null)
				return;

			foreach (RuntimeType type in this.typeSystem.GetAllTypes())
			{
				string plugTypeTarget = null;

				RuntimeAttribute typeAttribute = GetAttribute(type.CustomAttributes, plugTypeAttribute);

				if (typeAttribute != null)
				{
					object[] parameters = CustomAttributeParser.Parse(typeAttribute.Blob, typeAttribute.CtorMethod);

					if (parameters.Length >= 1)
					{
						plugTypeTarget = (string)parameters[0];
					}

				}

				foreach (RuntimeMethod method in type.Methods)
				{
					if (!method.IsStatic)
						continue;

					string plugMethodTarget = null;

					RuntimeAttribute methodAttribute = GetAttribute(method.CustomAttributes, plugMethodAttribute);

					if (methodAttribute != null)
					{
						object[] parameters = CustomAttributeParser.Parse(methodAttribute.Blob, methodAttribute.CtorMethod);

						if (parameters.Length >= 1)
						{
							plugMethodTarget = (string)parameters[0];
						}
					}

					if (plugTypeTarget != null || plugMethodTarget != null)
					{
						string targetAssemblyName;
						string targetFullTypeName;
						string targetMethodName;

						if (plugMethodTarget != null)
						{
							targetAssemblyName = ParseAssembly(plugMethodTarget);
							targetFullTypeName = ParseFullTypeName(plugMethodTarget);
							targetMethodName = ParseMethod(plugMethodTarget);
						}
						else
						{
							targetAssemblyName = ParseAssembly(plugTypeTarget);
							targetFullTypeName = RemoveAssembly(plugTypeTarget);
							targetMethodName = method.Name;
						}

						string targetNameSpace = ParseNameSpace(targetFullTypeName);
						string targetTypeName = ParseType(targetFullTypeName);

						RuntimeType targetType;

						if (targetAssemblyName != null)
							targetType = typeSystem.GetType(targetAssemblyName, targetNameSpace, targetTypeName);
						else
							targetType = typeSystem.GetType(targetNameSpace, targetTypeName);

						foreach (var targetMethod in targetType.Methods)
						{
							if (targetMethod.Name == targetMethodName)
							{
								if (targetMethod.IsStatic)
								{
									if (targetMethod.Signature.Matches(method.Signature))
									{
										plugMethods.Add(targetMethod, method);
										break;
									}
								}
								else
								{
									if (MatchesWithStaticThis(targetMethod, method))
									{
										plugMethods.Add(targetMethod, method);
										break;
									}
								}
							}
						}

					}
				}
			}
		}

		private RuntimeAttribute GetAttribute(List<RuntimeAttribute> attributes, RuntimeType plugAttribute)
		{
			foreach (RuntimeAttribute attribute in attributes)
			{
				if (attribute.Type == plugAttribute)
					return attribute;
			}

			return null;
		}

		private bool MatchesWithStaticThis(RuntimeMethod targetMethod, RuntimeMethod plugMethod)
		{
			MethodSignature target = targetMethod.Signature;
			MethodSignature plug = plugMethod.Signature;

			if (!target.ReturnType.Matches(plug.ReturnType))
				return false;

			if (target.Parameters.Length != plug.Parameters.Length - 1)
				return false;

			if (plug.Parameters[0].Type != Metadata.CilElementType.ByRef)
				return false;

			// TODO: Compare plug.Parameters[0].Type to the target's type

			for (int i = 0; i < target.Parameters.Length; i++)
			{
				if (!target.Parameters[i].Matches(plug.Parameters[i + 1]))
					return false;
			}

			return true;
		}

		private string RemoveAssembly(string target)
		{
			int pos = target.IndexOf(',');

			if (pos < 0)
				return target;

			return target.Substring(target.Length - pos - 1).Trim(' ');
		}

		private string ParseAssembly(string target)
		{
			int pos = target.IndexOf(',');

			if (pos < 0)
				return null;

			return target.Substring(pos + 1).Trim(' ');
		}

		private string ParseFullTypeName(string target)
		{
			target = RemoveAssembly(target);

			int pos = target.LastIndexOf('.');

			return target.Substring(0, pos);
		}

		private string ParseMethod(string target)
		{
			target = RemoveAssembly(target);

			int pos = target.LastIndexOf('.');

			return target.Substring(pos + 1);
		}

		private string ParseNameSpace(string type)
		{
			int pos = type.LastIndexOf('.');

			if (pos < 0)
				return string.Empty;

			return type.Substring(0, pos);
		}

		private string ParseType(string target)
		{
			int pos = target.LastIndexOf('.');

			return target.Substring(pos + 1);
		}
		#endregion // IAssemblyCompilerStage members


	}
}
