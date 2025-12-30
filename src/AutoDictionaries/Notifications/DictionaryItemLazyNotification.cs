using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Notifications
{
	public class DictionaryItemLazyNotification : INotificationHandler<DictionaryItemSavedNotification>,
													INotificationHandler<DictionaryItemDeletedNotification>
	{
		private readonly IAutoDictionariesService _autoDictionariesService;

		public DictionaryItemLazyNotification(IAutoDictionariesService autoDictionariesService)
		{
			_autoDictionariesService = autoDictionariesService;
		}

		public void Handle(DictionaryItemSavedNotification notification)
		{
			_autoDictionariesService.RefreshGetAllDictionaryItems();
		}

		public void Handle(DictionaryItemDeletedNotification notification)
		{
			_autoDictionariesService.RefreshGetAllDictionaryItems();
		}
	}
}
