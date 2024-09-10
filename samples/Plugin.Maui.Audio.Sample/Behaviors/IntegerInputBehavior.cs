namespace Plugin.Maui.Audio.Sample.Behaviors;

public class IntegerInputBehavior : Behavior<Entry>
{
	protected override void OnAttachedTo(Entry entry)
	{
		base.OnAttachedTo(entry);
		entry.TextChanged += OnEntryTextChanged;
	}

	protected override void OnDetachingFrom(Entry entry)
	{
		base.OnDetachingFrom(entry);
		entry.TextChanged -= OnEntryTextChanged;
	}

	void OnEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		var entry = sender as Entry;

		if (!string.IsNullOrWhiteSpace(e.NewTextValue) && !int.TryParse(e.NewTextValue, out _))
		{
			entry.Text = e.OldTextValue;
		}
	}
}
