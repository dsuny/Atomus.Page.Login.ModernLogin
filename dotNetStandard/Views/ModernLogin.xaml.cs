using Atomus.Page.Login.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Atomus.Page.Login
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModernLogin : ContentPage, ICore
    {
        #region INIT
        public ModernLogin()
        {
            this.BindingContext = new ModernLoginViewModel(this);

            InitializeComponent();
        }
        public ModernLogin(Application application) : this()
        {
            if (this.BindingContext != null)
                (this.BindingContext as ModernLoginViewModel).Application = application;
        }
        #endregion

        #region EVENT
        protected override void OnAppearing()
        {
            if ((this.BindingContext as ModernLoginViewModel).AutoLoginIsToggled)
                (this.BindingContext as ModernLoginViewModel).LoginCommand.Execute(null);
        }

        protected override bool OnBackButtonPressed()
        {
            this.Exit();
            return true;
        }
        #endregion

        #region ETC
        private async void Exit()
        {
            await (this.BindingContext as ModernLoginViewModel).Exit();
        }
        #endregion
    }
}