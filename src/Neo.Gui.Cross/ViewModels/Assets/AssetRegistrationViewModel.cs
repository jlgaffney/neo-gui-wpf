using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using Neo.Network.P2P.Payloads;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Assets
{
    public class AssetRegistrationViewModel :
        ViewModelBase,
        ILoadable
    {
        private readonly IAccountService accountService;

        private AssetType assetType;
        private string owner;
        private string admin;
        private string issuer;

        private string name;

        private bool totalIsLimited;
        private string totalLimit;

        private int precision = 8;

        public AssetRegistrationViewModel() { }
        public AssetRegistrationViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;

            AssetTypes = new ObservableCollection<AssetType>();
            Owners = new ObservableCollection<string>();
            Admins = new ObservableCollection<string>();
            Issuers = new ObservableCollection<string>();
        }

        // TODO Localise enum
        public ObservableCollection<AssetType> AssetTypes { get; }

        public ObservableCollection<string> Owners { get; }

        public ObservableCollection<string> Admins { get; }

        public ObservableCollection<string> Issuers { get; }

        public AssetType AssetType
        {
            get => assetType;
            set
            {
                if (assetType == value)
                {
                    return;
                }

                assetType = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
                this.RaisePropertyChanged(nameof(PrecisionEnabled));

                if (!PrecisionEnabled)
                {
                    Precision = 0;
                }
            }
        }

        public string Owner
        {
            get => owner;
            set
            {
                if (Equals(owner, value))
                {
                    return;
                }

                owner = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public string Admin
        {
            get => admin;
            set
            {
                if (Equals(admin, value))
                {
                    return;
                }

                admin = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public string Issuer
        {
            get => issuer;
            set
            {
                if (Equals(issuer, value))
                {
                    return;
                }

                issuer = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (Equals(name, value))
                {
                    return;
                }

                name = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public bool TotalIsLimited
        {
            get => totalIsLimited;
            set
            {
                if (Equals(totalIsLimited, value))
                {
                    return;
                }

                totalIsLimited = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(TotalLimit));
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public string TotalLimit
        {
            get => totalLimit;
            set
            {
                if (Equals(totalLimit, value))
                {
                    return;
                }

                totalLimit = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RegistrationEnabled));
            }
        }

        public int Precision
        {
            get => precision;
            set
            {
                if (Equals(precision, value))
                {
                    return;
                }

                precision = value;

                this.RaisePropertyChanged();
            }
        }

        public bool PrecisionEnabled => AssetType != AssetType.Share;

        public bool RegistrationEnabled =>
            !string.IsNullOrEmpty(Name) &&
            (!TotalIsLimited || !string.IsNullOrEmpty(TotalLimit)) &&
            Owner != null &&
            !string.IsNullOrWhiteSpace(Admin) &&
            !string.IsNullOrWhiteSpace(Issuer);

        public ReactiveCommand RegisterCommand => ReactiveCommand.Create(Register);
        
        public void Load()
        {
            AssetTypes.Add(AssetType.Share);
            AssetTypes.Add(AssetType.Token);

            var standardAccounts = accountService.GetStandardAccounts().ToList();
            var nonWatchOnlyAccounts = accountService.GetNonWatchOnlyAccounts().ToList();
            
            Owners.AddRange(standardAccounts.Select(account => account.GetKey().PublicKey.ToString()));
            Admins.AddRange(nonWatchOnlyAccounts.Select(account => account.Address));
            Issuers.AddRange(nonWatchOnlyAccounts.Select(account => account.Address));
        }

        private void Register()
        {
            if (!RegistrationEnabled)
            {
                return;
            }

            // TODO Build invocation transaction, sign it, and relay transaction to the network

            /*var assetRegistrationParameters = new AssetRegistrationTransactionParameters(
                AssetType,
                Name,
                TotalIsLimited,
                TotalLimit,
                Precision,
                Owner,
                Admin,
                Issuer);

            await this.walletController.BuildSignAndRelayTransaction(assetRegistrationParameters);*/

            OnClose();
        }
    }
}
