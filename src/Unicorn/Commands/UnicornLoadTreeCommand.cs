﻿using System;
using Rainbow.Model;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Shell.Framework.Commands.Serialization;
using Unicorn.Data;
using Unicorn.Logging;
using Unicorn.Pipelines.UnicornSyncEnd;
using Unicorn.Predicates;
using ItemData = Rainbow.Storage.Sc.ItemData;

namespace Unicorn.Commands
{
	public class UnicornLoadTreeCommand : LoadItemCommand
	{
		private readonly SerializationHelper _helper;

		public UnicornLoadTreeCommand() : this(new SerializationHelper())
		{

		}

		public UnicornLoadTreeCommand(SerializationHelper helper)
		{
			_helper = helper;
		}

		protected override Item LoadItem(Item item, LoadOptions options)
		{
			Assert.ArgumentNotNull(item, "item");

			IItemData itemData = new ItemData(item);

			var configuration = _helper.GetConfigurationForItem(itemData);

			if (configuration == null) return base.LoadItem(item, options);


			var logger = configuration.Resolve<ILogger>();
			var helper = configuration.Resolve<SerializationHelper>();
			var targetDataStore = configuration.Resolve<ITargetDataStore>();

			itemData = targetDataStore.GetByMetadata(itemData, itemData.DatabaseName);

			if (itemData == null)
			{
				logger.Warn("Command sync: Could not do partial sync of " + item.Paths.FullPath + " because the root was not serialized.");
				return item;
			}

			try
			{
				logger.Info("Command Sync: Processing partial Unicorn configuration " + configuration.Name + " under " + itemData.Path);

				helper.SyncTree(configuration, roots: new[] { itemData });

				logger.Info("Command Sync: Completed syncing partial Unicorn configuration " + configuration.Name + " under " + itemData.Path);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw;
			}

			CorePipeline.Run("unicornSyncEnd", new UnicornSyncEndPipelineArgs(configuration));

			return Database.GetItem(item.Uri);
		}
	}
}
