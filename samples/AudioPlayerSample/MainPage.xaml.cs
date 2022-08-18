using System.Reflection;
using Plugin.Maui.SimpleAudioPlayer;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{
    private readonly ISimpleAudioPlayer simpleAudioPlayer;
    int count = 0;

	public MainPage(ISimpleAudioPlayer simpleAudioPlayer)
	{
		InitializeComponent();
        this.simpleAudioPlayer = simpleAudioPlayer;
    }

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

        // TODO: attribute https://opengameart.org/content/epic-boss-battle
        simpleAudioPlayer.Load(GetStreamFromFile("colossal_3.mp3"));
		simpleAudioPlayer.Play();
	}

    Stream GetStreamFromFile(string filename)
    {
        var assembly = typeof(App).GetTypeInfo().Assembly;

        var stream = assembly.GetManifestResourceStream(filename);

        return stream;
    }
}


