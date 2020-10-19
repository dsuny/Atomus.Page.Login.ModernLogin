using Atomus.Page.Login.Controllers;
using Atomus.Security;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Atomus.Page.Login.ViewModel
{
    public class ModernLoginViewModel : MVVM.ViewModel
    {
        #region Declare
        private ICore core;
        private readonly string Configfilename;

        private string email;
        private string accessNumber;
        private bool rememberEmailIsToggled;
        private bool autoLoginIsToggled;
        private bool activityIndicator;
        private bool isEnabledControl;
        #endregion

        #region Property
        public Application Application { get; set; }
        public ICore Core
        {
            get
            {
                return this.core;
            }
            set
            {
                this.core = value;
                this.ConfigLoad();
            }
        }

        private Xamarin.Forms.Page JoinPage { get; set; }
        private Xamarin.Forms.Page PasswordChange { get; set; }

        public string AppName
        {
            get
            {
                return Config.Client.GetAttribute("App.Name").ToString();
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public string Eemail
        {
            get
            {
                return this.email;
            }
            set
            {
                if (this.email != value)
                {
                    this.email = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string AccessNumber
        {
            get
            {
                return this.accessNumber;
            }
            set
            {
                if (this.accessNumber != value)
                {
                    this.accessNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool RememberEmailIsToggled
        {
            get
            {
                return this.rememberEmailIsToggled;
            }
            set
            {
                if (this.rememberEmailIsToggled != value)
                {
                    if (!value)
                        this.AutoLoginIsToggled = value;

                    this.rememberEmailIsToggled = value;

                    NotifyPropertyChanged();

                    this.ConfigSave();
                }
            }
        }
        public bool AutoLoginIsToggled
        {
            get
            {
                return this.autoLoginIsToggled;
            }
            set
            {
                if (this.autoLoginIsToggled != value)
                {
                    if (value)
                        this.RememberEmailIsToggled = value;

                    this.autoLoginIsToggled = value;

                    NotifyPropertyChanged();

                    this.ConfigSave();
                }
            }
        }
        public bool ActivityIndicator
        {
            get
            {
                return this.activityIndicator;
            }
            set
            {
                if (this.activityIndicator != value)
                {
                    this.activityIndicator = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsEnabledControl
        {
            get
            {
                return this.isEnabledControl;
            }
            set
            {
                if (this.isEnabledControl != value)
                {
                    this.isEnabledControl = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ExitCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand PasswordChangeCommand { get; set; }
        public ICommand JoinCommand { get; set; }
        #endregion

        #region INIT
        public ModernLoginViewModel()
        {
            this.email = "";
            this.accessNumber = "";
            this.autoLoginIsToggled = false;
            this.autoLoginIsToggled = false;
            this.activityIndicator = false;
            this.isEnabledControl = true;

            this.Configfilename = Path.Combine(Factory.FolderPath, $"DefaultLoginStandard.config");

            this.ExitCommand = new Command(async () => await this.Exit()
                                            , () => { return !this.ActivityIndicator; });

            this.LoginCommand = new Command(async () => await this.LoginProcess()
                                            , () => { return !this.ActivityIndicator; });

            this.PasswordChangeCommand = new Command(async () => await this.PasswordChangeProcess()
                                            , () => { return !this.ActivityIndicator; });

            this.JoinCommand = new Command(async () => await this.JoinProcess()
                                            , () => { return !this.ActivityIndicator; });
        }
        public ModernLoginViewModel(ICore core) : this()
        {
            this.Core = core;
        }
        #endregion

        #region IO
        private void ConfigLoad()
        {
            IDecryptor decryptor;

            try
            {
                if (File.Exists(this.Configfilename))
                {
                    string tmp = File.ReadAllText(Configfilename);
                    string[] tmps = tmp.Split(',');

                    this.rememberEmailIsToggled = tmps[0].Split(':')[1].Length > 0;
                    this.autoLoginIsToggled = tmps[1].Split(':')[1].Length > 0;

                    if (this.rememberEmailIsToggled)
                        this.email = tmps[0].Split(':')[1];

                    if (this.autoLoginIsToggled && this.email.Length > 0 && tmps[0].Split(':')[1].Length > 0)
                    {
                        decryptor = (IDecryptor)this.Core.CreateInstance("Decryptor");

                        this.accessNumber = decryptor.DecryptFromBase64String(tmps[1].Split(':')[1], this.Eemail, this.Eemail);
                    }
                }
                else
                {
                    this.ConfigSave();
                }
            }
            catch (Exception ex)
            {
                Diagnostics.DiagnosticsTool.MyTrace(ex);
            }
        }
        private void ConfigSave()
        {
            IEncryptor encryptor;

            try
            {
                if (this.Eemail.Length > 0 && this.AccessNumber.Length > 0)
                {
                    if (this.RememberEmailIsToggled && this.AutoLoginIsToggled)
                    {
                        encryptor = (IEncryptor)this.Core.CreateInstance("Encryptor");

                        File.WriteAllText(this.Configfilename, $"EMAIL:{(this.RememberEmailIsToggled ? this.Eemail : "")},ACCESS_NUMBER:{encryptor.EncryptToBase64String(this.AccessNumber, this.Eemail, this.Eemail)}");
                    }
                    else if (this.RememberEmailIsToggled)
                    {
                        File.WriteAllText(this.Configfilename, $"EMAIL:{(this.RememberEmailIsToggled ? this.Eemail : "")},ACCESS_NUMBER:");
                    }
                    else
                        File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
                }
                else
                    File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
            }
            catch (Exception ex)
            {
                Diagnostics.DiagnosticsTool.MyTrace(ex);
                File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
            }
        }

        private async Task LoginProcess()
        {
            Service.IResponse result;
            ISecureHashAlgorithm secureHashAlgorithm;
            string tmp;

            try
            {
                this.IsEnabledControl = false;
                this.ActivityIndicator = true;
                (this.LoginCommand as Command).ChangeCanExecute();

                secureHashAlgorithm = (ISecureHashAlgorithm)this.core.CreateInstance("SecureHashAlgorithm");

                result = await this.core.SearchAsync(this.email, secureHashAlgorithm.ComputeHashToBase64String(this.accessNumber));

                if (result.Status == Service.Status.OK)
                {
                    this.ConfigSave();

                    if (result.DataSet != null && result.DataSet.Tables.Count >= 1)
                        foreach (DataTable _DataTable in result.DataSet.Tables)
                            for (int i = 1; i < _DataTable.Columns.Count; i++)
                                foreach (DataRow _DataRow in _DataTable.Rows)
                                {
                                    tmp = string.Format("{0}.{1}", _DataRow[0].ToString(), _DataTable.Columns[i].ColumnName);

                                    if (Config.Client.GetAttribute(tmp) == null)
                                        Config.Client.SetAttribute(tmp, _DataRow[i]);
                                }


                    if (this.Application.MainPage is NavigationPage)
                    {
                        this.Application.MainPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser");
                        //this.Application.MainPage = new Browser.ModernBrowser();
                    }
                    else
                        await this.Exit(false);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Warning", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                this.ActivityIndicator = false;
                this.IsEnabledControl = true;
                (this.LoginCommand as Command).ChangeCanExecute();
            }
        }

        private async Task PasswordChangeProcess()
        {
            try
            {
                this.IsEnabledControl = false;
                this.ActivityIndicator = true;
                (this.PasswordChangeCommand as Command).ChangeCanExecute();

                if (this.PasswordChange == null)
                {
                    this.PasswordChange = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultPasswordChange");
                    //this.PasswordChange = new PasswordChange.ModernPasswordChange();
                }

                await ((NavigationPage)this.Application.MainPage).PushAsync(this.PasswordChange);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                this.ActivityIndicator = false;
                this.IsEnabledControl = true;
                (this.PasswordChangeCommand as Command).ChangeCanExecute();
            }
        }

        private async Task JoinProcess()
        {
            try
            {
                this.IsEnabledControl = false;
                this.ActivityIndicator = true;
                (this.JoinCommand as Command).ChangeCanExecute();

                if (this.JoinPage == null)
                {
                    this.JoinPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultJoin");
                    //this.JoinPage = new Join.ModernJoin();
                }

                await ((NavigationPage)this.Application.MainPage).PushAsync(this.JoinPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                this.ActivityIndicator = false;
                this.IsEnabledControl = true;
                (this.JoinCommand as Command).ChangeCanExecute();
            }
        }

        internal async Task Exit(bool isQuestions = true)
        {
            bool result;

            this.IsEnabledControl = false;
            this.ActivityIndicator = true;
            (this.ExitCommand as Command).ChangeCanExecute();

            if (isQuestions)
                result = await Application.Current.MainPage.DisplayAlert("종료", "종료하시겠습니까??", "예", "아니요");
            else
                result = true;

            if (result)
            {
                DependencyService.Get<INativeHelper>().CloseApp();
            }

            this.ActivityIndicator = false;
            this.IsEnabledControl = true;
            (this.ExitCommand as Command).ChangeCanExecute();
        }
#endregion

#region ETC
#endregion
    }
}