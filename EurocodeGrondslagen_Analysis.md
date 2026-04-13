# EurocodeGrondslagen Component Analyse

## Overzicht
Een Blazor Razor component voor het weergeven en beheren van Eurocode grondslagen parameters. Dit component maakt gebruik van Fluent UI componenten en biedt een interface voor het configureren van nationale bijlagen, gevolgklasses en ontwerplevensduur.

## Technische Details
- **Bestandslocatie**: `EurocodeRazorClassLibrary\Grondslagen\EurocodeGrondslagen.razor`
- **Framework**: Blazor (.NET 9)
- **C# Versie**: 13.0

## Dependencies
```csharp
@using ExportFactory.Interfaces.Export
@using ExportFactory.Services
@using Eurocode.Grondslagen
```

## Component Structuur

### Parameters

| Parameter | Type | Required | Default | Beschrijving |
|-----------|------|----------|---------|--------------|
| `Model` | `GrondslagenContext` | Ja (required) | - | Het data model met grondslagen informatie |
| `ModelChanged` | `EventCallback<GrondslagenContext>` | Nee | - | Event callback voor two-way binding |
| `ToonGevolgEigenschappen` | `bool` | Nee | `false` | Bepaalt of gevolg eigenschappen worden getoond |
| `ReadOnly` | `bool` | Nee | `false` | Bepaalt of de velden read-only zijn |

### UI Elementen

#### 1. FluentSelectEnum - Nationale Bijlage
```razor
<FluentSelectEnum Label="nationale bijlage" 
                  ReadOnly="ReadOnly"
                  Placeholder="Selecteer..."
                  TEnum="Eurocode.Grondslagen.NationaleBijlageEnum"
                  ShowDescription="true" 
                  @bind-EnumSelectedValue="@Model.NationaleBijlage" />
```
- **Doel**: Selectie van de nationale bijlage
- **Type**: `NationaleBijlageEnum`
- **Features**: Toont beschrijving, gebruikt two-way binding

#### 2. FluentSelectEnum - Gevolgklasse
```razor
<FluentSelectEnum Label="gevolgklasse (Consequence Class)"
                  ReadOnly="ReadOnly"
                  Placeholder="Selecteer..."
                  TEnum="Eurocode.Grondslagen.GevolgklasseEnum"
                  ShowDescription="false"
                  @bind-EnumSelectedValue="@Model.Gevolgklasse" />
```
- **Doel**: Selectie van de gevolgklasse (Consequence Class)
- **Type**: `GevolgklasseEnum`
- **Features**: Geen beschrijving getoond, gebruikt two-way binding

#### 3. FluentSelectEnum - Ontwerplevensduur
```razor
<FluentSelectEnum Label="ontwerplevensduur"
                  ReadOnly="ReadOnly"
                  TEnum="Eurocode.Grondslagen.OntwerpLevensduurEnum"
                  @bind-EnumSelectedValue="@Model.OntwerpLevensduur" />
```
- **Doel**: Selectie van de ontwerplevensduur
- **Type**: `OntwerpLevensduurEnum`
- **Features**: Gebruikt two-way binding

### Conditionele Weergave

Wanneer `ToonGevolgEigenschappen` is `true`, worden de volgende eigenschappen getoond:

#### 1. Betrouwbaarheidsklasse
```razor
<EurocodeInputContainer ReadOnly=true Key="Betrouwbaarheidsklasse">
    <ChildContent>
       @(Model.Betrouwbaarheidsklasse.ToString())
    </ChildContent>
</EurocodeInputContainer>
```
- Read-only weergave
- Toont als string

#### 2. Kfi Factor
```razor
<EurocodeInputContainer ReadOnly=true Key="Kfi">
    <ChildContent>
        @(Model.Kfi.ToString("0.0"))
    </ChildContent>
</EurocodeInputContainer>
```
- Read-only weergave
- Geformatteerd met 1 decimaal (0.0)

#### 3. Xi Factor
```razor
<EurocodeInputContainer ReadOnly=true Disabled=true Key="Xi">
    <ChildContent>
        @(Model.Xi.ToString("0.00"))
    </ChildContent>
</EurocodeInputContainer>
```
- Read-only én disabled
- Geformatteerd met 2 decimalen (0.00)

## Methods

### OnInputChanged()
```csharp
private async Task OnInputChanged()
{
    // Trigger the event to notify the parent of the model change
    await ModelChanged.InvokeAsync(Model);
}
```
- **Status**: ?? Niet gebruikt in de huidige implementatie
- **Doel**: Event triggeren naar parent component
- **Gebruik**: Zou kunnen worden gebruikt voor custom change handling

### UpdateModel()
```csharp
async Task UpdateModel()
{
    // so we can use @bind-Model
    await ModelChanged.InvokeAsync(Model);
}
```
- **Status**: ?? Niet gebruikt in de huidige implementatie
- **Doel**: Model updates propageren voor @bind-Model scenario's
- **Gebruik**: Voorbereid voor toekomstige @bind-Model functionaliteit

## Data Flow

```
???????????????????????????????????????????
?         Parent Component                ?
???????????????????????????????????????????
            ? Model (GrondslagenContext)
            ? ToonGevolgEigenschappen
            ? ReadOnly
            ?
???????????????????????????????????????????
?    EurocodeGrondslagen Component        ?
?                                          ?
?  ??????????????????????????????????    ?
?  ?  FluentSelectEnum Components   ?    ?
?  ?  (Two-way binding)             ?    ?
?  ??????????????????????????????????    ?
?                                          ?
?  ??????????????????????????????????    ?
?  ?  Conditionale Weergave         ?    ?
?  ?  (Read-only eigenschappen)     ?    ?
?  ??????????????????????????????????    ?
???????????????????????????????????????????
            ? ModelChanged Event
            ?
???????????????????????????????????????????
?         Parent Component                ?
???????????????????????????????????????????
```

## Observaties en Aanbevelingen

### ? Sterke Punten
1. **Two-way binding**: Goed gebruik van `@bind-EnumSelectedValue` voor automatische synchronisatie
2. **Type-safe**: Gebruik van strongly-typed enum parameters
3. **Flexibel**: Configureerbaar via parameters (ReadOnly, ToonGevolgEigenschappen)
4. **Consistent**: Consistente naamgeving en structuur

### ?? Potentiële Verbeterpunten
1. **Ongebruikte Methods**: `OnInputChanged()` en `UpdateModel()` worden niet aangeroepen
2. **Inconsistente Properties**: 
   - `Betrouwbaarheidsklasse`: ReadOnly=true
   - `Kfi`: ReadOnly=true
   - `Xi`: ReadOnly=true én Disabled=true (waarom beide?)
3. **Missing Placeholder**: De derde FluentSelectEnum heeft geen Placeholder property
4. **ShowDescription Inconsistentie**: 
   - Nationale bijlage: `ShowDescription="true"`
   - Gevolgklasse: `ShowDescription="false"`
   - Ontwerplevensduur: geen ShowDescription property
5. **Hardcoded Formatting**: Format strings ("0.0", "0.00") zijn hardcoded

### ?? Mogelijke Issues
1. De `OnInputChanged` method wordt nergens aangeroepen - mogelijk dead code
2. De `UpdateModel` method wordt nergens aangeroepen - mogelijk dead code
3. Geen validatie logica zichtbaar
4. Geen error handling

## Eurocode Context

Dit component implementeert de basis grondslagen volgens:
- **Nationale Bijlagen**: Landspecifieke aanpassingen van Eurocodes
- **Gevolgklassen (CC)**: CC1, CC2, CC3 volgens EN 1990
- **Ontwerplevensduur**: Categorieën volgens EN 1990 Tabel 2.1
- **Betrouwbaarheidsklasse (RC)**: RC1, RC2, RC3
- **Kfi**: Betrouwbaarheidsfactor
- **Xi**: Gevoeligheidscoëfficiënt voor onzekerheden

## Gebruik Voorbeeld

```razor
<EurocodeGrondslagen Model="@grondslagenModel"
                     ModelChanged="@OnGrondslagenChanged"
                     ToonGevolgEigenschappen="true"
                     ReadOnly="false" />

@code {
    private GrondslagenContext grondslagenModel = new();
    
    private async Task OnGrondslagenChanged(GrondslagenContext context)
    {
        // Handle model changes
        await DoSomethingWithUpdatedModel(context);
    }
}
```

## Afhankelijkheden

### Custom Components
- `FluentSelectEnum`: Enum dropdown component
- `EurocodeInputContainer`: Container voor Eurocode input velden

### Data Models
- `GrondslagenContext`: Hoofdmodel met alle grondslagen data
- `NationaleBijlageEnum`: Enum voor nationale bijlagen
- `GevolgklasseEnum`: Enum voor gevolgklassen
- `OntwerpLevensduurEnum`: Enum voor ontwerplevensduur categorieën

---

**Analyse datum**: Gegenereerd op verzoek  
**Component versie**: .NET 9, C# 13.0  
**Framework**: Blazor
