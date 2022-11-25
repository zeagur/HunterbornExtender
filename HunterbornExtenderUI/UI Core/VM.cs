using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Noggog;

namespace HunterbornExtenderUI;

public class VM : INotifyPropertyChanged, IDisposableDropoff
{
    private readonly CompositeDisposable _compositeDisposable = new();
#pragma warning disable CS8612 // Nullability of reference types in type doesn't match implicitly implemented member.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8612 // Nullability of reference types in type doesn't match implicitly implemented member.

    public void Dispose() => _compositeDisposable.Dispose();

    public void Add(IDisposable disposable)
    {
        _compositeDisposable.Add(disposable);
    }

    // Create the OnPropertyChanged method to raise the event 
    protected void OnPropertyChanged(string name)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(name));
        }
    }
}