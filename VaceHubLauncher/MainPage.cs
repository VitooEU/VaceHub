using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VaceHub {
 public sealed class Game {
  public string Title, System, Notes; public bool Favorite;
  public JsonObject Json() => new JsonObject { ["title"] = JsonValue.CreateStringValue(Title), ["system"] = JsonValue.CreateStringValue(System), ["notes"] = JsonValue.CreateStringValue(Notes), ["favorite"] = JsonValue.CreateBooleanValue(Favorite) };
 }
 public sealed class MainPage : Page {
  readonly System.Threading.SemaphoreSlim saveGate = new System.Threading.SemaphoreSlim(1,1);
  readonly string[] systems = { "PS1", "PS2", "PSP", "N64", "Game Boy", "Game Boy Color", "Game Boy Advance", "NES", "SNES", "Mega Drive", "Dreamcast", "Xbox (Original)" };
  List<Game> games = new List<Game>();
  readonly StackPanel cards = new StackPanel { Spacing = 12 };
  readonly TextBlock count = new TextBlock();
  readonly TextBlock notice = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brush("#B2FF5A"), FontSize = 18 };
  string filter = "Alle"; bool favorites; bool dialogOpen; bool storageHealthy = true;
  TextBox search; Button home;
  static SolidColorBrush Brush(string hex) { return new SolidColorBrush(Color.FromArgb(255, Convert.ToByte(hex.Substring(1,2),16), Convert.ToByte(hex.Substring(3,2),16), Convert.ToByte(hex.Substring(5,2),16))); }
  static TextBlock Text(string s, double size = 20) => new TextBlock { Text = s, FontSize = size, TextWrapping = TextWrapping.Wrap };
  Button Button(string label, Action action) { var b = new Button { Content = label, Padding = new Thickness(18,12,18,12), Margin = new Thickness(0,0,8,8), FontSize = 19, UseSystemFocusVisuals = true }; b.Click += (s,e) => action(); return b; }
  public MainPage() {
   RequiresPointer = RequiresPointer.Never;
   var root = new Grid { Background = Brush("#0B0E12"), Padding = new Thickness(48,30,48,30) };
   root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) }); root.ColumnDefinitions.Add(new ColumnDefinition());
   var side = new StackPanel { Spacing = 8, Margin = new Thickness(0,0,28,0) };
   side.Children.Add(new Image { Source = new BitmapImage(new Uri("ms-appx:///Assets/Mark.png")), Height = 100, HorizontalAlignment = HorizontalAlignment.Left });
   side.Children.Add(Text("VACEHUB",28)); side.Children.Add(Text("DEINE RETRO-ZENTRALE",12));
   home = Button("Bibliothek", () => { filter="Alle"; favorites=false; Render(); }); side.Children.Add(home);
   side.Children.Add(Button("★ Favoriten", () => { favorites=true; Render(); }));
   side.Children.Add(Button("＋ Spiel erfassen", async () => await Add()));
   side.Children.Add(Button("Über VaceHub", async () => await Info("VaceHub • Version 1.0", "Bibliothek und Controller-Oberfläche sind implementiert. Keine Emulatoren, BIOS-Dateien oder Spiele enthalten. Keine Emulator-Startintegration eingerichtet. Xbox-Original ist nur eine Sammlungskategorie. Installation: siehe README.md im Projekt.")));
   root.Children.Add(side);
   var body = new Grid(); Grid.SetColumn(body,1); root.Children.Add(body);
   body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); body.RowDefinitions.Add(new RowDefinition()); body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
   var top = new StackPanel { Spacing = 12 };
   var hero = new Grid { Height = 180, Background = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Background.png")), Stretch = Stretch.UniformToFill } };
   hero.Children.Add(new Border { Background = Brush("#101419"), Opacity = .65 });
   var headline = new StackPanel { Margin = new Thickness(24,18,24,12) }; headline.Children.Add(Text("DEIN NÄCHSTES ABENTEUER.",30)); headline.Children.Add(Text("Eine Bibliothek. Alle deine Retro-Welten.",19)); headline.Children.Add(Text("SAMMLUNG BEREIT  /  EMULATOREN NICHT VERBUNDEN",14)); hero.Children.Add(headline); top.Children.Add(hero);
   search = new TextBox { PlaceholderText = "Spiele suchen …", FontSize = 20 }; search.TextChanged += (s,e) => Render(); top.Children.Add(search);
   var categories = new StackPanel { Orientation = Orientation.Horizontal };
   foreach (var name in new[] { "Alle" }.Concat(systems)) { var n=name; categories.Children.Add(Button(n,()=> { filter=n; favorites=false; Render(); })); }
   top.Children.Add(new ScrollViewer { Content = categories, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, HorizontalScrollMode = ScrollMode.Enabled, VerticalScrollMode = ScrollMode.Disabled });
   top.Children.Add(count); top.Children.Add(notice); body.Children.Add(top);
   var list = new ScrollViewer { Content = cards, Margin = new Thickness(0,16,0,16), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }; Grid.SetRow(list,1); body.Children.Add(list);
   var footer = Text("✚ / Stick  Navigieren     A  Auswählen     B / Esc  Bibliothek",16); Grid.SetRow(footer,2); body.Children.Add(footer);
   Content = root; Loaded += async (s,e) => { await Load(); Render(); home.Focus(FocusState.Programmatic); };
   KeyDown += OnKey;
  }
  void OnKey(object sender, KeyRoutedEventArgs e) {
   if (dialogOpen) return;
   if (e.Key == Windows.System.VirtualKey.GamepadB || e.Key == Windows.System.VirtualKey.Escape) { filter="Alle"; favorites=false; search.Text=""; Render(); home.Focus(FocusState.Programmatic); e.Handled=true; }
  }
  async Task Info(string title, string text) {
   if (dialogOpen) return; dialogOpen=true;
   try { await new ContentDialog { Title=title, Content=Text(text), CloseButtonText="Zurück" }.ShowAsync(); } finally { dialogOpen=false; }
  }
  void Render() {
   if (search==null) return;
   var visible=games.Where(g=>(filter=="Alle" || g.System==filter) && (!favorites || g.Favorite) && g.Title.IndexOf(search.Text, StringComparison.CurrentCultureIgnoreCase)>=0).OrderBy(g=>g.Title).ToList();
   count.Text=(favorites ? "Favoriten" : filter=="Alle" ? "Deine Bibliothek" : filter) + "  /  " + visible.Count + " Spiele";
   cards.Children.Clear();
   if (visible.Count==0) { cards.Children.Add(Text("Hier beginnt deine Sammlung.",28)); cards.Children.Add(Text("Erfasse eigene Spiele über „Spiel erfassen“. Einträge verwalten Metadaten; sie starten noch keine ROMs.",20)); }
   foreach (var game in visible) {
    var row = new StackPanel { Spacing=8, Padding=new Thickness(20), Background=Brush("#182027") };
    row.Children.Add(Text((game.Favorite ? "★  " : "")+game.Title,25)); row.Children.Add(Text(game.System+"  ·  Kein Emulator verbunden",16));
    var actions=new StackPanel { Orientation=Orientation.Horizontal };
    actions.Children.Add(Button("Details",async ()=>await Info(game.Title,game.System+"\n\n"+game.Notes+"\n\nStart nicht verfügbar: Für dieses System ist keine Emulatorintegration implementiert.")));
    actions.Children.Add(Button(game.Favorite ? "★ Entfernen" : "☆ Favorit", async ()=> { game.Favorite=!game.Favorite; await Save(); Render(); }));
    actions.Children.Add(Button("Eintrag löschen",async ()=>await Remove(game))); row.Children.Add(actions); cards.Children.Add(row);
   }
  }
  async Task Add() {
   if (dialogOpen) return; dialogOpen=true;
   try {
    var title=new TextBox { Header="Spieltitel", MaxLength=120 }; var system=new ComboBox { Header="System", ItemsSource=systems, SelectedIndex=0, HorizontalAlignment=HorizontalAlignment.Stretch };
    var notes=new TextBox { Header="Notizen / Hinweis zum Speicherort", AcceptsReturn=true, MaxLength=2000, Height=110 };
    var content=new StackPanel { Spacing=16 }; content.Children.Add(title); content.Children.Add(system); content.Children.Add(notes); content.Children.Add(Text("Nur Bibliothekseintrag – kein ROM-Import und kein Spielstart.",16));
    var dialog=new ContentDialog { Title="Spiel erfassen", Content=content, PrimaryButtonText="Speichern", CloseButtonText="Abbrechen", IsPrimaryButtonEnabled=false };
    title.TextChanged+=(s,e)=>dialog.IsPrimaryButtonEnabled=!string.IsNullOrWhiteSpace(title.Text);
    if (await dialog.ShowAsync()==ContentDialogResult.Primary) { games.Add(new Game { Title=title.Text.Trim(), System=(string)system.SelectedItem, Notes=notes.Text }); await Save(); Render(); }
   } finally { dialogOpen=false; }
  }
  async Task Remove(Game game) {
   if (dialogOpen) return; dialogOpen=true;
   try { var d=new ContentDialog { Title="Eintrag löschen?", Content=Text(game.Title+" wird aus der Bibliothek entfernt. Spieldateien werden nicht verändert."), PrimaryButtonText="Löschen", CloseButtonText="Abbrechen" }; if (await d.ShowAsync()==ContentDialogResult.Primary) { games.Remove(game); await Save(); Render(); } } finally { dialogOpen=false; }
  }
  async Task Load() {
   try {
    var file=await ApplicationData.Current.LocalFolder.TryGetItemAsync("library.json") as StorageFile; if (file==null) return;
    var loaded=new List<Game>(); foreach (var value in JsonArray.Parse(await FileIO.ReadTextAsync(file))) { var j=value.GetObject(); var sys=j.GetNamedString("system"); if (!systems.Contains(sys)) throw new FormatException("Unbekanntes System"); loaded.Add(new Game { Title=j.GetNamedString("title"), System=sys, Notes=j.GetNamedString("notes",""), Favorite=j.GetNamedBoolean("favorite",false) }); } games=loaded;
   } catch (Exception ex) { storageHealthy=false; notice.Text="Bibliothek konnte nicht geladen werden. Original bleibt erhalten; Speichern gesperrt. "+ex.Message; }
  }
  async Task Save() {
   if (!storageHealthy) { notice.Text="Speichern gesperrt: library.json zuerst sichern und reparieren (siehe README)."; return; }
   await saveGate.WaitAsync();
   try { var json=new JsonArray(); foreach (var g in games) json.Add(g.Json()); var folder=ApplicationData.Current.LocalFolder; var temp=await folder.CreateFileAsync("library.pending.json",CreationCollisionOption.ReplaceExisting); await FileIO.WriteTextAsync(temp,json.Stringify(),Windows.Storage.Streams.UnicodeEncoding.Utf8); var target=await folder.TryGetItemAsync("library.json") as StorageFile; if (target==null) await temp.RenameAsync("library.json"); else await temp.MoveAndReplaceAsync(target); notice.Text="Bibliothek gespeichert."; }
   catch (Exception ex) { notice.Text="Speichern fehlgeschlagen – Änderungen nur in dieser Sitzung: "+ex.Message; }
   finally { saveGate.Release(); }
  }
 }
}
