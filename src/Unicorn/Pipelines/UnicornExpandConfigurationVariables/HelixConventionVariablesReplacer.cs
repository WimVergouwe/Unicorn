﻿using System;
using System.Collections.Generic;
using Configy.Parsing;

namespace Unicorn.Pipelines.UnicornExpandConfigurationVariables
{
	/// <summary>
	/// Enables the use of $(layer) and $(module) in configurations, as long as you name the configurations to Helix conventions (e.g. Layer.Module)
	/// This can allow you to use a single base configuration that defines your storage and predicate conventions so you don't have to restate them all day.
	/// </summary>
	public class HelixConventionVariablesReplacer : ContainerDefinitionVariablesReplacer, IUnicornExpandConfigurationVariablesProcessor
	{
		public void Process(UnicornExpandConfigurationVariablesPipelineArgs args)
		{
			ReplaceVariables(args.Configuration);
		}

		public override void ReplaceVariables(ContainerDefinition definition)
		{
			if (definition.Name == null) throw new ArgumentException("Configuration without a name was used. Add a name attribute to all configurations.", nameof(definition));

			var variables = GetVariables(definition.Name);

			ApplyVariables(definition.Definition, variables);
		}

		public virtual Dictionary<string, string> GetVariables(string name)
		{
			var pieces = name.Split('.');

			if (pieces.Length < 2) return new Dictionary<string, string>();

			return new Dictionary<string, string>
			{
				{"layer", pieces[0]},
				{"module", pieces[1]}
			};
		}
	}
}