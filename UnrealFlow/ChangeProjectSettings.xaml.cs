namespace UnrealFlow{

	[QueryProperty( nameof( id ), "id" )]
	public partial class ChangeProjectSettings : ContentPage{
		public ChangeProjectSettings(){
			InitializeComponent();
			if( this.id == "" ){
				this.nameHeader.Text = "Add Project";
			}
			else {
				this.nameHeader.Text = "Project settings xxxx";
			}
		}

		public void OnCancel( object sender, EventArgs e ){
      Shell.Current.GoToAsync( ".." );
    }

		public void OnLookupProjectPath( object sender, EventArgs e ){
			
		}

		public void OnApply( object sender, EventArgs e ){
			if( id == "" ){

			}
			else {
				
			}
      Shell.Current.GoToAsync( ".." );
    }

		public string id { get; set; } = "";
	}
};