using BotApi.Interfaces;
using static BotApi.Commands.RegisterCommands;

namespace BotApi.Commands
{
    public interface ICommandFactory
    {
        ICommand Create(CommandType type);
    }

    public class CommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICommand Create(CommandType type)
        {
            switch (type)
            {
                case CommandType.ComicSubscribe:
                    return null;

                case CommandType.FincanceSubscribe:
                    return null;

                case CommandType.Coins:
                    return _serviceProvider.GetRequiredService<GetCoinsCommand>();

                case CommandType.WakeOnLan:
                    return null;

                case CommandType.Balance:
                    return null;

                case CommandType.BalanceAdd:
                    return null;

                case CommandType.BalanceRemove:
                    return null;

                case CommandType.BalanceDetails:
                    return null;

                case CommandType.QBitTorrentSearch:
                    return _serviceProvider.GetRequiredService<QBSearchCommand>();

                case CommandType.QBPlugins:
                    return _serviceProvider.GetRequiredService<QBPluginsCommand>();

                case CommandType.QBEnablePlugin:
                    return _serviceProvider.GetRequiredService<QBEnablePluginCommand>();

                case CommandType.QBDisablePlugin:
                    return _serviceProvider.GetRequiredService<QBDisablePluginCommand>();

                case CommandType.QBDownloadTorrentCallback:
                    return _serviceProvider.GetRequiredService<QBDownloadTorrentCallbackCommand>();

                case CommandType.QBProgress:
                    return _serviceProvider.GetRequiredService<QBProgressCommand>();

				case CommandType.MusifyDownloadAlbum:
					return _serviceProvider.GetRequiredService<MusifyDownloadAlbumCommand>();

				case CommandType.MusifyDownloadSong:
					return _serviceProvider.GetRequiredService<MusifyDownloadSongCommand>();

				case CommandType.Unknown:
                    return null;

                default:
                    return null;
            }
        }
    }

    public static class RegisterCommands
    {
        internal static void Register(IServiceCollection services)
        {
            var commandDictionary = new Dictionary<CommandType, ICommand>
            {
                // The factory will provide the implementation
                { CommandType.Coins, null },
                { CommandType.ComicSubscribe, null },
                { CommandType.FincanceSubscribe, null },
                { CommandType.WakeOnLan, null },
                { CommandType.Balance, null },
                { CommandType.BalanceAdd, null },
                { CommandType.BalanceRemove, null },
                { CommandType.BalanceDetails, null },
                { CommandType.QBitTorrentSearch, null },
                { CommandType.QBPlugins, null },
                { CommandType.QBEnablePlugin, null },
                { CommandType.QBDisablePlugin, null },
                { CommandType.QBDownloadTorrentCallback, null },
                { CommandType.QBProgress, null },
                { CommandType.MusifyDownloadAlbum, null },
                { CommandType.MusifyDownloadSong, null },
			};

            services.AddSingleton(commandDictionary);

            services.AddSingleton<GetCoinsCommand>();
            services.AddSingleton<QBSearchCommand>();
            services.AddSingleton<QBPluginsCommand>();
            services.AddSingleton<QBEnablePluginCommand>();
            services.AddSingleton<QBDisablePluginCommand>();
            services.AddSingleton<QBDownloadTorrentCallbackCommand>();
            services.AddSingleton<QBProgressCommand>();
            services.AddSingleton<MusifyDownloadAlbumCommand>();
            services.AddSingleton<MusifyDownloadSongCommand>();

		}

        public enum CommandType
        {
            Coins,
            ComicSubscribe,
            FincanceSubscribe,
            WakeOnLan,
            Balance,
            BalanceAdd,
            BalanceRemove,
            BalanceDetails,
            QBitTorrentSearch,
            QBPlugins,
            QBEnablePlugin,
            QBDisablePlugin,
            QBDownloadTorrentCallback,
            QBProgress,
            MusifyDownloadAlbum,
            MusifyDownloadSong,
			Unknown,
        }

        public static Dictionary<string, CommandType> CommandsText = new Dictionary<string, CommandType>
        {
            {"/subs", CommandType.ComicSubscribe },
            {"/finance", CommandType.FincanceSubscribe },
            {"/coins", CommandType.Coins },
            {"/c", CommandType.Coins },
            {"/wol", CommandType.WakeOnLan },
            {"/balance", CommandType.Balance },
            {"/b", CommandType.Balance },
            {"/balanceadd", CommandType.BalanceAdd },
            {"/balanceremove", CommandType.BalanceRemove },
            {"/balancedetails", CommandType.BalanceDetails },
            {"/bd", CommandType.BalanceDetails },
            {"/qbsearch", CommandType.QBitTorrentSearch },
            {"/qbplugins", CommandType.QBPlugins },
            {"/qbenableplugin", CommandType.QBEnablePlugin },
            {"/qbdisableplugin", CommandType.QBDisablePlugin },
            {"/qdc", CommandType.QBDownloadTorrentCallback },
            {"/qbprogress", CommandType.QBProgress },
            {"/mdlalbum", CommandType.MusifyDownloadAlbum },
            {"/mdlsong", CommandType.MusifyDownloadSong },

		};
    }
}
