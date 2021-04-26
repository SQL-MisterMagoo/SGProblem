using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BlazorComponentGenerator
{
	[Generator]
	internal class SimpleComponentGenerator : ISourceGenerator
	{
		private const string attributeSource = "[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]\r" +
				"internal sealed class SourceGeneratedBlazorAttribute: System.Attribute\r" +
				"{\r" +
				"    public string Name { get; }\r" +
				"    public SourceGeneratedBlazorAttribute(string name)\r" +
				"        => Name = name;\r" +
				"}\r";
		private const string sbAttributeName = "SourceGeneratedBlazorAttribute";

		private readonly DiagnosticDescriptor _foundComponent =
			new("BCG0001", "Found Component", $"{DateTime.Now:HH:mm:ss.fff} : Component Reference : {{0}}",
			"Source Generator", DiagnosticSeverity.Info, true);

		private readonly Assembly assembly;

		public SimpleComponentGenerator()
		{
			assembly = GetType().Assembly;
		}

		public void Execute(GeneratorExecutionContext context)
		{

			IEnumerable<AttributeData> compAss = context.Compilation.Assembly.GetAttributes().Where(ad => ad.AttributeClass.Name == sbAttributeName);
			File.AppendAllText("c:\\temp\\diagnostics.log", $"Attribute Count: {compAss.Count()}\r");
			foreach (AttributeData attr in compAss)
			{
				File.AppendAllText("c:\\temp\\diagnostics.log", $"Attribute : {attr.ApplicationSyntaxReference.GetSyntax().GetText()}\r");
				ImmutableArray<TypedConstant> arguments = attr.ConstructorArguments;
				if (arguments.Length == 1)
				{
					var componentName = arguments[0].Value.ToString();
					File.AppendAllText("c:\\temp\\diagnostics.log", $"Component: {componentName}\r");
					try
					{
						if (!string.IsNullOrWhiteSpace(componentName))
						{
							using Stream stream = assembly.GetManifestResourceStream($"BlazorComponentGenerator.ComponentTemplates._{componentName}.razor.cs");
							if (stream is not null)
							{
								using StreamReader reader = new(stream);
								var result = reader.ReadToEnd();
								context.AddSource(componentName, result);
								context.ReportDiagnostic(Diagnostic.Create(_foundComponent, attr.ApplicationSyntaxReference?.GetSyntax().GetLocation(), componentName));
							}
						}
					}
					catch (Exception ex)
					{
						File.AppendAllText("c:\\temp\\diagnostics.log", $"{ex.GetBaseException().Message}\r");
					}
				}
			}
			//if (context.SyntaxReceiver is ComponentSyntaxReceiver sr)
			//{

			//	foreach (var item in sr.NameCandidates)
			//	{
			//	TODO: find proper roslyn syntax for this

			//   var attributeItem = ((AttributeSyntax)item);
			//   var constantValue = context.Compilation.GetSemanticModel(item.SyntaxTree).GetConstantValue(attributeItem.ArgumentList.Arguments[0]);
			//		var componentName = constantValue.HasValue ? constantValue.Value.ToString() : string.Empty;
			//		((AttributeSyntax)item).ArgumentList.Arguments[0].GetText().ToString().Trim('"');
			//		File.AppendAllText("c:\\temp\\diagnostics.log", $"Argument: {attributeItem.ArgumentList.Arguments[0]}\r");

			//		try
			//		{
			//			using Stream stream = assembly.GetManifestResourceStream($"BlazorComponentGenerator.ComponentTemplates._{componentName}.razor.cs");
			//			using StreamReader reader = new StreamReader(stream);
			//			string result = reader.ReadToEnd();
			//			context.AddSource(componentName, result);
			//		}
			//		catch (Exception ex)
			//		{
			//			File.AppendAllText("c:\\temp\\diagnostics.log", $"{ex.GetBaseException().Message}\r");
			//		}
			//	}
			//}
		}

		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForPostInitialization(context
				=> context.AddSource("SourceGeneratedBlazor_MainAttributes__", SourceText.From(attributeSource, Encoding.UTF8))
				);
			//context.RegisterForSyntaxNotifications(() => new ComponentSyntaxReceiver());
		}
	}
}
