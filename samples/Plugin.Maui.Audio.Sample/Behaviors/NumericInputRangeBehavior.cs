namespace Plugin.Maui.Audio.Sample.Behaviors;

public class NumericInputRangeBehavior : Behavior<Entry>
{
	public static readonly BindableProperty MinValueProperty = BindableProperty.Create(nameof(MinValue), typeof(double), typeof(NumericInputRangeBehavior), double.MinValue);

	public double MinValue
	{
		get => (double)GetValue(MinValueProperty);
		set => SetValue(MinValueProperty, value);
	}

	public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(nameof(MaxValue), typeof(double), typeof(NumericInputRangeBehavior), double.MaxValue);

	public double MaxValue
	{
		get => (double)GetValue(MaxValueProperty);
		set => SetValue(MaxValueProperty, value);
	}

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

		if (!string.IsNullOrWhiteSpace(e.NewTextValue))
		{
			if (double.TryParse(e.NewTextValue, out double value))
			{
				if (value < MinValue)
				{
					entry.Text = MinValue.ToString();
				}
				else if (value > MaxValue)
				{
					entry.Text = MaxValue.ToString();
				}
			}
		}
	}
}
