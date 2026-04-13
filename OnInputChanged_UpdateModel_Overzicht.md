# Overzicht: Waar OnInputChanged() en UpdateModel() aan te roepen

## ?? Doel van de Methods

### `OnInputChanged()`
Method om parent component te notificeren wanneer het model wijzigt via custom event handling.

### `UpdateModel()`
Method om model updates te propageren, specifiek voorbereid voor `@bind-Model` scenario's.

---

## ?? Locaties waar je deze methods kunt gebruiken

### 1. **Binnen EurocodeGrondslagen.razor Component**

#### Optie A: Event handlers toevoegen aan FluentSelectEnum
Je kunt de `OnStateChanged` EventCallback van `FluentSelectEnum` gebruiken:

```razor
<FluentSelectEnum Label="nationale bijlage" 
                  ReadOnly="ReadOnly"
                  Placeholder="Selecteer..."
                  TEnum="Eurocode.Grondslagen.NationaleBijlageEnum"
                  ShowDescription="true" 
                  @bind-EnumSelectedValue="@Model.NationaleBijlage"
                  OnStateChanged="@OnInputChanged" />
```

**Bestand**: `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor`

#### Optie B: PropertyChanged event gebruiken (zoals in EurocodeBetonScheurwijdteComponent)

```csharp
@code {
    protected override void OnInitialized()
    {
        if (Model is INotifyPropertyChanged observable)
        {
            observable.PropertyChanged += ModelPropertyChanged;
        }
    }

    private async void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await OnInputChanged();
    }
    
    public void Dispose()
    {
        if (Model is INotifyPropertyChanged observable)
        {
            observable.PropertyChanged -= ModelPropertyChanged;
        }
    }
}
```

**Bestand**: `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor`

---

### 2. **In Parent Components die EurocodeGrondslagen gebruiken**

#### A. GrondslagenDefault.razor
**Bestand**: `examples\Eurocode.Blazor.Demo.Shared\Pages\Grondslagen\Examples\GrondslagenDefault.razor`

**Huidige code:**
```razor
<EurocodeGrondslagen @bind-Model="@SampleProject.Grondslagen"
                     ToonGevolgEigenschappen=@ToonReadOnlyFields  />
```

**Optie 1: Gebruik ModelChanged callback**
```razor
<EurocodeGrondslagen Model="@SampleProject.Grondslagen"
                     ModelChanged="@OnGrondslagenChanged"
                     ToonGevolgEigenschappen=@ToonReadOnlyFields  />

@code {
    private async Task OnGrondslagenChanged(GrondslagenContext context)
    {
        SampleProject.Grondslagen = context;
        // Hier kun je extra logica toevoegen
        await GenerateNewDocumentAsync();
        StateHasChanged();
    }
}
```

**Optie 2: Behoud @bind-Model en gebruik private method**
```razor
<EurocodeGrondslagen @bind-Model="@SampleProject.Grondslagen"
                     ToonGevolgEigenschappen=@ToonReadOnlyFields  />

@code {
    private GrondslagenContext _grondslagen
    {
        get => SampleProject?.Grondslagen;
        set
        {
            if (SampleProject != null)
            {
                SampleProject.Grondslagen = value;
                OnGrondslagenUpdated();
            }
        }
    }
    
    private void OnGrondslagenUpdated()
    {
        GenerateNewDocument();
        StateHasChanged();
    }
}
```

---

### 3. **In GrondslagenContext.cs (Model Class)**

**Bestand**: `Eurocode0.Grondslagen\GrondslagenContext.cs`

Je kunt `INotifyPropertyChanged` interface implementeren:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class GrondslagenContext : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private NationaleBijlageEnum _nationaleBijlage;
    public NationaleBijlageEnum NationaleBijlage
    {
        get => _nationaleBijlage;
        set
        {
            if (_nationaleBijlage != value)
            {
                _nationaleBijlage = value;
                OnPropertyChanged();
                // Trigger herberekening indien nodig
                BerekenAfgeleideWaarden();
            }
        }
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void BerekenAfgeleideWaarden()
    {
        // Herbereken Kfi, Xi, etc.
    }
}
```

---

## ?? Aanbevolen Implementatie Strategie

### **Strategie 1: Event-driven binnen component** (Meest direct)

1. **Bestand wijzigen**: `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor`

```razor
<FluentSelectEnum Label="nationale bijlage" 
                  ReadOnly="ReadOnly"
                  Placeholder="Selecteer..."
                  TEnum="Eurocode.Grondslagen.NationaleBijlageEnum"
                  ShowDescription="true" 
                  @bind-EnumSelectedValue="@Model.NationaleBijlage"
                  OnStateChanged="@OnInputChanged" />

<FluentSelectEnum Label="gevolgklasse (Consequence Class)"
                  ReadOnly="ReadOnly"
                  Placeholder="Selecteer..."
                  TEnum="Eurocode.Grondslagen.GevolgklasseEnum"
                  ShowDescription="false"
                  @bind-EnumSelectedValue="@Model.Gevolgklasse"
                  OnStateChanged="@OnInputChanged" />

<FluentSelectEnum Label="ontwerplevensduur"
                  ReadOnly="ReadOnly"
                  TEnum="Eurocode.Grondslagen.OntwerpLevensduurEnum"
                  @bind-EnumSelectedValue="@Model.OntwerpLevensduur"
                  OnStateChanged="@OnInputChanged" />
```

**Voordeel**: Direct gekoppeld aan UI changes  
**Nadeel**: Moet op elke FluentSelectEnum worden toegevoegd

---

### **Strategie 2: INotifyPropertyChanged in Model** (Meest robuust)

1. **Bestand wijzigen**: `Eurocode0.Grondslagen\GrondslagenContext.cs`
   - Implementeer `INotifyPropertyChanged` op alle relevante properties

2. **Bestand wijzigen**: `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor`
   - Subscribe op PropertyChanged events in `OnInitialized()`
   - Call `OnInputChanged()` in de event handler

**Voordeel**: Werkt voor alle property changes, ongeacht UI component  
**Nadeel**: Meer code in model class

---

### **Strategie 3: Gebruik in Parent Component** (Voor specifieke use cases)

1. **Bestand wijzigen**: `examples\Eurocode.Blazor.Demo.Shared\Pages\Grondslagen\Examples\GrondslagenDefault.razor`
   - Gebruik `ModelChanged` callback
   - Trigger custom logica zoals document regeneratie

**Voordeel**: Scheiding van concerns, logica blijft in parent  
**Nadeel**: Moet in elke parent worden geïmplementeerd

---

## ?? Concrete Acties per Bestand

### ? Minimale wijziging (Quick win)

| Bestand | Actie |
|---------|-------|
| `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor` | Voeg `OnStateChanged="@OnInputChanged"` toe aan alle 3 FluentSelectEnum components |

### ? Optimale implementatie (Aanbevolen)

| Bestand | Actie |
|---------|-------|
| `Eurocode0.Grondslagen\GrondslagenContext.cs` | Implementeer `INotifyPropertyChanged` interface |
| `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor` | Subscribe op PropertyChanged events en call `OnInputChanged()` |
| `examples\Eurocode.Blazor.Demo.Shared\Pages\Grondslagen\Examples\GrondslagenDefault.razor` | Gebruik `ModelChanged` callback voor document regeneratie |

---

## ?? Bestaande Voorbeelden in Codebase

### EurocodeBetonScheurwijdteComponent.razor
**Bestand**: `EurocodeRazorClassLibrary\Beton\EurocodeBetonScheurwijdteComponent.razor`

Dit component laat zien hoe PropertyChanged events worden gebruikt:

```csharp
protected override void OnInitialized()
{
    if (Model!.Wapening is INotifyPropertyChanged observable)
    {
        observable.PropertyChanged += WapeningPropertyChanged;
    }
    Model.BerekenEnValideer();
}

private void WapeningPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    Model!.BerekenEnValideer();
    StateHasChanged();
    ModelChanged.InvokeAsync(Model);
}
```

Dit patroon kun je toepassen in `EurocodeGrondslagen.razor`.

---

## ?? Gebruikscenario's

### Scenario 1: Herberekening bij wijziging
```csharp
private async Task OnInputChanged()
{
    // Herbereken afgeleide waarden
    Model.BerekenKfiEnXi();
    
    // Notificeer parent
    await ModelChanged.InvokeAsync(Model);
}
```

### Scenario 2: Validatie bij wijziging
```csharp
private async Task OnInputChanged()
{
    // Valideer input
    var validationResult = Model.Valideer();
    
    if (!validationResult.IsValid)
    {
        // Toon validatie errors
    }
    
    await ModelChanged.InvokeAsync(Model);
}
```

### Scenario 3: Logging/Tracking
```csharp
private async Task OnInputChanged()
{
    // Log wijziging
    Logger.LogInformation($"Grondslagen gewijzigd: {Model.NationaleBijlage}");
    
    await ModelChanged.InvokeAsync(Model);
}
```

---

## ? Quick Reference

| Als je wilt... | Wijzig bestand... | Gebruik method... |
|----------------|-------------------|-------------------|
| Event per UI change | `EurocodeGrondslagen.razor` | `OnInputChanged()` met `OnStateChanged` |
| Event per property change | `GrondslagenContext.cs` | `OnInputChanged()` via PropertyChanged handler |
| Custom logica in parent | `GrondslagenDefault.razor` | Eigen method via `ModelChanged` callback |
| @bind-Model support | `EurocodeGrondslagen.razor` | `UpdateModel()` met property setter |

---

**Status**: Deze methods zijn momenteel **NIET** in gebruik  
**Aanbeveling**: Start met **Strategie 1** voor snelle implementatie of **Strategie 2** voor robuuste oplossing
