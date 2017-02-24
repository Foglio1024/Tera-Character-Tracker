﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media.Effects;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using Tera.Converters;
using System.Xml.Linq;
using TCTData.Enums;
using Tera.Controls;
using TCTUI;

namespace Tera
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>

    public partial class TeraMainWindow : Window
    {
        public TeraMainWindow()
        {
            InitializeComponent();

            TeraLogic.UndoList = new List<Delegate>();

            Top = TCTData.TCTProps.Top;
            Left = TCTData.TCTProps.Left;
            Height = TCTData.TCTProps.Height;
            Width = TCTData.TCTProps.Width;

            var buttonStyle = new Style { TargetType = typeof(System.Windows.Shapes.Rectangle) };
            buttonStyle.Setters.Add(new Setter(HeightProperty, 20.0));
            buttonStyle.Setters.Add(new Setter(WidthProperty, 20.0));
            buttonStyle.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
            buttonStyle.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            buttonStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Center));
            buttonStyle.Setters.Add(new Setter(OpacityProperty, .3));
            buttonStyle.Setters.Add(new EventSetter(MouseEnterEvent, new MouseEventHandler(BarButtonHoverIn)));
            buttonStyle.Setters.Add(new EventSetter(MouseLeaveEvent, new MouseEventHandler(BarButtonHoverOut)));
            buttonStyle.Setters.Add(new Setter(MarginProperty, new Thickness(3,0,0,0)));
            switch (TCTData.TCTProps.Theme)
            {
                case Theme.Light:
                    MainGrid.Background = new SolidColorBrush(TCTData.Colors.LightTheme_Background);
                    ToolBar.Background = new SolidColorBrush(TCTData.Colors.LightTheme_Bar);
                    ToolBar.Effect = TCTData.Shadows.LightThemeShadow;
                    title.Foreground = new SolidColorBrush(TCTData.Colors.LightTheme_Foreground1);
                    buttonStyle.Setters.Add(new Setter(System.Windows.Shapes.Rectangle.FillProperty, new SolidColorBrush(TCTData.Colors.DarkTheme_Card)));
                    break;
                case Theme.Dark:
                    MainGrid.Background = new SolidColorBrush(TCTData.Colors.DarkTheme_Background);
                    ToolBar.Background = new SolidColorBrush(TCTData.Colors.DarkTheme_Bar);
                    ToolBar.Effect = TCTData.Shadows.DarkThemeShadow;
                    title.Foreground = new SolidColorBrush(TCTData.Colors.DarkTheme_Foreground1);
                    buttonStyle.Setters.Add(new Setter(System.Windows.Shapes.Rectangle.FillProperty, new SolidColorBrush(TCTData.Colors.LightTheme_Card)));

                    break;
                default:
                    break;
            }
            this.Resources["toolBarButton"] = buttonStyle;
            //MinWidth = main.chView.Width + main.accountsColumn.MinWidth;

            //NI = new System.Windows.Forms.NotifyIcon();
            //NI.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            //NI.Icon = Tera.Properties.Resources.tctlogo;
            //NI.Text = "Tera Character Tracker " + TCTData.TCTProps.CurrentVersion;
            //UI.NotifyIcon = NI;
            UI.MainWin = this;
        }
        bool leftSlideIsOpen = false;
        bool isLogExpanded = false;
        //static System.Windows.Forms.NotifyIcon NI;
        Popup dgWindow = new Popup();
        public DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
        public DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(100));
        public DoubleAnimation glowIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
        public DoubleAnimation glowOut = new DoubleAnimation( .3, TimeSpan.FromMilliseconds(150));
        public DoubleAnimation expand = new DoubleAnimation(40, TimeSpan.FromMilliseconds(100));

        double accountsInitialWidth = 715;

        //public static List<CharacterStrip> CharacterStrips { get; set; } = new List<CharacterStrip>();
        public static List<ExtendedCharacterStrip> ExtendedCharacterStrips { get; set; } = new List<ExtendedCharacterStrip>();
        public static List<DungeonRunsCounter> DungeonCounters { get; set; } = new List<DungeonRunsCounter>();

        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        //private void CreateStripControlsBindings(int i)
        //{

        //    /*creates text boxes data bindings*/
        //    CharacterStrips[i].nameTB.SetBinding(TextBlock.TextProperty, DataBinder.GenericCharBinding(i, "Name"));
        //    CharacterStrips[i].lvlTB.SetBinding(TextBlock.TextProperty, DataBinder.GenericCharBinding(i, "Level"));
        //    DataBinder.BindParameterToBarGauge(i, "Credits", CharacterStrips[i].creditsTB, TeraLogic.MAX_CREDITS, TeraLogic.MAX_CREDITS - 300, true, true);
        //    DataBinder.BindParameterToBarGauge(i, "MarksOfValor", CharacterStrips[i].mvTB, TeraLogic.MAX_MARKS, TeraLogic.MAX_MARKS-10, true, true);
        //    DataBinder.BindParameterToBarGauge(i, "GoldfingerTokens", CharacterStrips[i].gftTB, TeraLogic.MAX_GF_TOKENS, TeraLogic.MAX_GF_TOKENS-10, true, true);
        //    DataBinder.BindParameterToQuestBarGauge(i, "Weekly", "Dailies", CharacterStrips[i].questTB, TeraLogic.MAX_WEEKLY, TeraLogic.MAX_WEEKLY - TeraLogic.MAX_DAILY, TeraLogic.MAX_DAILY, TeraLogic.MAX_DAILY, true, false);
        //    DataBinder.BindParameterToImageSourceWithConverter(i, "CharClass", CharacterStrips[i].classImage, "sd", new ClassToImage());
        //    DataBinder.BindCharPropertyToShapeFillColor(i, "Laurel", CharacterStrips[i].laurelRect, new Laurel_GradeToColor());
        //    DataBinder.BindParameterToArcGauge(i, "Crystalbind", CharacterStrips[i].ccbInd, new TimeToAngle());
        //    CharacterStrips[i].ccbInd.SetBinding(ToolTipProperty, DataBinder.GenericCharBinding(i, "Crystalbind", new TicksToTimespan(), null));
        //    CharacterStrips[i].SetBinding(TagProperty, DataBinder.GenericCharBinding(i, "Name"));
            
        //}
        //public void AddStripToContainer(int i)
        //{
        //    /*adds strip to panel*/
        //    (CharacterStrips[i].Content as Grid).Height = 1;
        //    UI.CharListContainer.chContainer.Items.Add(CharacterStrips[i]);
        //    (CharacterStrips[i].Content as Grid).BeginAnimation(FrameworkElement.HeightProperty, expand);
        //}
        //public void CreateStrip(int i)
        //{
        //    /*adds strip to array*/
        //    CharacterStrips.Add(new CharacterStrip());

        //    /*create bindings for controls*/
        //    CreateStripControlsBindings(i);

        //    /*add strip to container*/
        //    AddStripToContainer(i);

        //}
        public void CreateExtStrip(int i)
        {
            /*adds strip to array*/
            ExtendedCharacterStrips.Add(new ExtendedCharacterStrip());

            /*create bindings for controls*/
            CreateExtendedStripControlsBindings(i);

            /*add strip to container*/
            AddExtendedStripToContainer(i);

        }
        public void AddExtendedStripToContainer(int i)
        {
            /*adds strip to panel*/
            //(ExtendedCharacterStrips[i].Content as Grid).Height = 1;
            UI.CharListContainer.chContainer.Items.Add(ExtendedCharacterStrips[i]);
            //(ExtendedCharacterStrips[i].Content as Grid).BeginAnimation(FrameworkElement.HeightProperty, expand);
        }

        private void CreateExtendedStripControlsBindings(int i)
        {

            /*creates text boxes data bindings*/
            ExtendedCharacterStrips[i].nameTB.SetBinding(TextBlock.TextProperty, DataBinder.GenericCharBinding(i, "Name"));
            ExtendedCharacterStrips[i].lvlTB.SetBinding(TextBlock.TextProperty, DataBinder.GenericCharBinding(i, "Level"));
            ExtendedCharacterStrips[i].ilvlTB.SetBinding(TextBlock.TextProperty, DataBinder.GenericCharBinding(i, "Ilvl"));

            DataBinder.BindParameterToBarGauge(i, "Credits", ExtendedCharacterStrips[i].creditsTB, TeraLogic.MAX_CREDITS, TeraLogic.MAX_CREDITS - 300, true, true);
            DataBinder.BindParameterToBarGauge(i, "MarksOfValor", ExtendedCharacterStrips[i].mvTB, TeraLogic.MAX_MARKS, TeraLogic.MAX_MARKS - 10, true, true);
            DataBinder.BindParameterToBarGauge(i, "GoldfingerTokens", ExtendedCharacterStrips[i].gftTB, TeraLogic.MAX_GF_TOKENS, TeraLogic.MAX_GF_TOKENS - 10, true, true);
            DataBinder.BindParameterToBarGauge(i, "DragonwingScales", ExtendedCharacterStrips[i].scTB, TeraLogic.MAX_DRAGONWING_SCALES, TeraLogic.MAX_DRAGONWING_SCALES - 5, true, true);

            DataBinder.BindParameterToQuestBarGauge(i, "Weekly", "Dailies", ExtendedCharacterStrips[i].questTB, TeraLogic.MAX_WEEKLY, TeraLogic.MAX_WEEKLY - TeraLogic.MAX_DAILY, TeraLogic.MAX_DAILY, TeraLogic.MAX_DAILY, true, false);
            DataBinder.BindParameterToOpacityMaskImageSourceWithConverter(i, "CharClass", ExtendedCharacterStrips[i].classImage, "sd", new ClassToBrush());
            DataBinder.BindCharPropertyToShapeFillColor(i, "Laurel", ExtendedCharacterStrips[i].laurelRect, new Laurel_GradeToColor());
            DataBinder.BindParameterToArcGauge(i, "Crystalbind", ExtendedCharacterStrips[i].ccbInd, new TimeToAngle());
            ExtendedCharacterStrips[i].ccbInd.SetBinding(ToolTipProperty, DataBinder.GenericCharBinding(i, "Crystalbind", new TicksToTimespan(), null));
            ExtendedCharacterStrips[i].SetBinding(TagProperty, DataBinder.GenericCharBinding(i, "Name"));

        }

        public void SetGuildImage(Bitmap logo)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                MemoryStream stream = new MemoryStream();

                logo.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                UI.CharView.guildLogo.Source = result;


            }));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            

            /*creates strips for loaded chars*/
            for (int i = 0; i < TeraLogic.CharList.Count; i++)
            {
                //CreateStrip(i);
                CreateExtStrip(i);
            }
               

            TeraLogic.IsSaved = true;

            foreach (var dg in TeraLogic.DungList)
            {
                DungeonRunsCounter d = new DungeonRunsCounter();
                d.Name = dg.ShortName;
                d.Tag = dg.ShortName;
                d.n.Text = dg.ShortName;
                XElement dgNameEl = TCTData.TCTDatabase.StrSheet_Dungeon.Descendants().Where(x => (string)x.Attribute("id") == dg.Id.ToString()).FirstOrDefault();
                if (dgNameEl != null)
                {
                    d.ToolTip = dgNameEl.Attribute("string").Value;
                }

                DungeonCounters.Add(d);

                DungeonClearsCounter c = new DungeonClearsCounter();
                c.Name = dg.ShortName;
                c.Tag = dg.ShortName;
                c.dungeonName.Text = dg.ShortName;
                XElement dgNameEl1 = TCTData.TCTDatabase.StrSheet_Dungeon.Descendants().Where(x => (string)x.Attribute("id") == dg.Id.ToString()).FirstOrDefault();
                if (dgNameEl != null)
                {
                    c.ToolTip = dgNameEl.Attribute("string").Value;
                }

                switch (dg.Tier)
                {
                    case DungeonTier.Tier2:
                        UI.CharView.t2panel.Children.Add(d);
                        UI.CharView.t2panelC.Children.Add(c);
                        break;

                    case DungeonTier.Tier3:
                        UI.CharView.t3panel.Children.Add(d);
                        UI.CharView.t3panelC.Children.Add(c);

                        break;

                    case DungeonTier.Tier4:
                        UI.CharView.tier4panel.Children.Add(d);
                        UI.CharView.t4panelC.Children.Add(c);

                        break;

                    case DungeonTier.Tier5:
                        UI.CharView.tier5panel.Children.Add(d);
                        UI.CharView.t5panelC.Children.Add(c);

                        break;

                    case DungeonTier.Solo:
                        UI.CharView.soloPanel.Children.Add(d);

                        break;

                    default:
                        break;
                }
            }

            this.StatusBar.Background =          new SolidColorBrush(TCTData.Colors.SolidBaseColor);
            Log.BorderThickness = new Thickness(0);
            UI.CharView.guildGrid.Background =   new SolidColorBrush(TCTData.Colors.SolidBaseColor);
            this.MinWidth = main.accountsColumn.MinWidth + main.chView.Width;

            this.Activate();

        }
        private void LogSizeToggle(object sender, MouseButtonEventArgs e)
        {
            var s = sender as Grid;
            var expand = new DoubleAnimationUsingKeyFrames();
            expand.KeyFrames.Add(new SplineDoubleKeyFrame(145, TimeSpan.FromMilliseconds(150), new KeySpline(.5, 0, .3, 1)));
            var shrink = new DoubleAnimationUsingKeyFrames();
            shrink.KeyFrames.Add(new SplineDoubleKeyFrame(20, TimeSpan.FromMilliseconds(150), new KeySpline(.5, 0, .3, 1)));

            expand.Completed += (se, ex) => {
                Log.Items.MoveCurrentToLast();
                Log.ScrollIntoView(Log.Items.CurrentItem);
            };

            shrink.Completed += (se, ex) =>
            {
                Log.Items.MoveCurrentToLast();
                Log.ScrollIntoView(Log.Items.CurrentItem);
            };

            /*convert to animation*/
            if (!isLogExpanded)
            {
                s.BeginAnimation(HeightProperty, expand);
                isLogExpanded = true;
            }
            else 
            {
                s.BeginAnimation(HeightProperty, shrink);
                isLogExpanded = false;
            }
        } 
        private void SaveButtonPressed(object sender, RoutedEventArgs e)
        {
            TeraLogic.SaveCharacters(false);
            TeraLogic.SaveAccounts(false);
            TeraLogic.SaveGuildsDB(false);
            TeraLogic.SaveSettings(false);
            UI.UpdateLog("Data saved.");
            TeraLogic.IsSaved = true;
        }
        private void LeftSlideToggle(object sender, MouseButtonEventArgs e)
        {
             ThicknessAnimationUsingKeyFrames an = new ThicknessAnimationUsingKeyFrames();

            if (!leftSlideIsOpen)
            {
                an.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0), TimeSpan.FromMilliseconds(220), new KeySpline(.5, 0, .3, 1)));
                leftSlideIsOpen = true;
            }
            else
            {
                an.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(-420,0,0,0), TimeSpan.FromMilliseconds(220), new KeySpline(.5, 0, .3, 1)));
                leftSlideIsOpen = false;
            }

            leftSlide1.BeginAnimation(MarginProperty, an);

        }
        private void ShowDungeonOverview(object sender, RoutedEventArgs e)
        {
            
            var w = dgWindow;
            w.MouseLeave += (s0, e0) =>
             {
                 w.IsOpen = false;
             };
            if (w.IsOpen)
            {
                w.IsOpen = false;
            }
            else
            {
                /*add char names in left column*/

                w.Child = new DungeonsWindow();
                w.AllowsTransparency = true;
                w.PlacementTarget = sender as System.Windows.Controls.Image;
                w.Placement = PlacementMode.Right;
                foreach (var c in TeraLogic.CharList)
                {
                    if (c.Level == 65)
                    {
                        (w.Child as DungeonsWindow).chars.Children.Add(new TextBlock { Text = c.Name, Height = 20, Foreground = new SolidColorBrush { Color = new System.Windows.Media.Color { A = 130, R = 0, G = 0, B = 0 } } });
                    }
                }


                /*add dungeons short names in header (also "Characters")*/
                foreach (var dg in TeraLogic.DungList)
                {
                    (w.Child as DungeonsWindow).header.Children.Add(new TextBlock { Text = dg.ShortName, TextAlignment=TextAlignment.Center, Width = 30, Foreground = new SolidColorBrush { Color = new System.Windows.Media.Color { A = 130, R = 0, G = 0, B = 0 } }, VerticalAlignment = VerticalAlignment.Center });
                }

                var localList = new List<Character>();


                /*add dungeon circles indicators in main panel*/
                foreach (TextBlock c in (w.Child as DungeonsWindow).chars.Children)
                {
                    Character character = TeraLogic.CharList.Find(x => x.Name == c.Text);
                    if (character.Level == 65)
                    {
                        localList.Add(character);
                        var sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;
                        foreach (var dg in TeraLogic.DungList)
                        {
                            var g = new Grid();
                            
                            g.Width = 30;
                            g.Height = 20;
                            g.Background = new SolidColorBrush(new System.Windows.Media.Color{ A = 255, R = 255, G = 255, B = 255 });
                            var el = new Ellipse();
                            var t = new TextBlock();
                            el.Width = 15;
                            el.Height = 15;
                            t.FontSize = 11;
                            t.Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 190, R = 255, G = 255, B = 255 });
                            el.HorizontalAlignment = HorizontalAlignment.Center;
                            el.VerticalAlignment = VerticalAlignment.Center;
                            t.HorizontalAlignment = HorizontalAlignment.Center;
                            t.VerticalAlignment = VerticalAlignment.Center;
                            t.TextAlignment = TextAlignment.Center;
                            t.FontWeight = FontWeights.DemiBold;
                            el.Fill = new SolidColorBrush(new System.Windows.Media.Color { A = 255, R = 0, G = 0, B = 0 });
                            int max = dg.MaxBaseRuns;
                            if (TeraLogic.AccountList.Find(a => a.Id == character.AccountId).TeraClub)
                            {
                                if(dg.ShortName != "CA" && dg.ShortName != "GL" && dg.ShortName != "AH")
                                {
                                    max = dg.MaxBaseRuns * 2;
                                }
                            }
                            int dgIndex = character.Dungeons.IndexOf(character.Dungeons.Find(d => d.Name.Equals(dg.ShortName)));
                            var b = new Binding
                            {
                                Source = character.Dungeons[dgIndex],
                                Path = new PropertyPath("Runs"),
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                Mode = BindingMode.OneWay,
                                Converter = new Dungeon_RunsToColor(),
                                ConverterParameter = max,

                            };
                            var tb = new Binding
                            {
                                Source = character.Dungeons[dgIndex],
                                Path = new PropertyPath("Runs"),
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                Mode = BindingMode.OneWay,
                            };
                            t.SetBinding(TextBlock.TextProperty, tb);
                            el.SetBinding(Shape.FillProperty, b);

                            g.Children.Add(el);
                            g.Children.Add(t);
                            g.Tag = character.Name +"%"+ dg.ShortName;
                            g.MouseEnter += (snd, evn) =>
                            {
                                var parameters = (snd as Grid).Tag.ToString().Split('%');
                                string chName = parameters[0];
                                string dgName = parameters[1];
                                int chIndex = localList.IndexOf(localList.Find(x => x.Name.Equals(chName)));
                                int dgIndex0 = TeraLogic.DungList.IndexOf(TeraLogic.DungList.Find(x => x.ShortName.Equals(dgName)));
                                if (chIndex < (w.Child as DungeonsWindow).chars.Children.Count)
                                {
                                    (((w.Child as DungeonsWindow).chars.Children[chIndex] as TextBlock)).Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 255, R = 0, G = 0, B = 0 });
                                    (((w.Child as DungeonsWindow).header.Children[dgIndex0] as TextBlock)).Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 255, R = 0, G = 0, B = 0 });
                                }
                            };


                            g.MouseLeave += (snd, evn) =>
                            {
                                var parameters = (snd as Grid).Tag.ToString().Split('%');
                                string chName = parameters[0];
                                string dgName = parameters[1];
                                int chIndex = localList.IndexOf(localList.Find(x => x.Name.Equals(chName)));
                                int dgIndex0 = TeraLogic.DungList.IndexOf(TeraLogic.DungList.Find(x => x.ShortName.Equals(dgName)));
                                if(chIndex < (w.Child as DungeonsWindow).chars.Children.Count)
                                {
                                    (((w.Child as DungeonsWindow).chars.Children[chIndex] as TextBlock)).Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 130, R = 0, G = 0, B = 0 });
                                    (((w.Child as DungeonsWindow).header.Children[dgIndex0] as TextBlock)).Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 130, R = 0, G = 0, B = 0 });
                                }
                            };



                            sp.Children.Add(g);


                        }

                    (w.Child as DungeonsWindow).content.Children.Add(sp);

                    }
                }



                w.IsOpen = true;
            }
        }
        private void Win_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TeraLogic.SaveCharacters(false);
            TeraLogic.SaveAccounts(false);
            TeraLogic.SaveGuildsDB(false);

            TCTData.TCTProps.Top = this.Top;
            TCTData.TCTProps.Left = this.Left;
            TCTData.TCTProps.Height = this.ActualHeight;
            TCTData.TCTProps.Width = this.ActualWidth;
            TeraLogic.SaveSettings(false);

            
        }
        private void TeraMainWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private delegate void UndoDelegate();
        private void UndoDeletion()
        {
            if(TeraLogic.DeletedChars.Count > 0)
            {
                TeraLogic.AddCharacter(TeraLogic.DeletedChars.Last());
                TeraLogic.DeletedChars.Remove(TeraLogic.DeletedChars.Last());
            }
        }
        private void UndoWeeklyReset()
        {
            foreach (var item in TeraLogic.ResettedWeekly)
            {
                TeraLogic.CharList.Find(c => c.Name == item.Key).Weekly = item.Value;
            }
            TeraLogic.ResettedWeekly.Clear();
        }
        private void UndoDailyReset()
        {
            foreach (var item in TeraLogic.ResettedDailies)
            {
                TeraLogic.CharList.Find(c => c.Name == item.Key).Dailies = item.Value;
            }
            TeraLogic.ResettedDailies.Clear();
        }

        private void UndoButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (TeraLogic.UndoList.Count > 0)
            {
                TeraLogic.UndoList.Last().DynamicInvoke();
                TeraLogic.UndoList.RemoveAt(TeraLogic.UndoList.Count - 1);
            }

        }

        int i = 0;
        private void TestButton(object sender, MouseButtonEventArgs e)
        {
            string teststring = "Test" + i;

           UI.SendNotification(teststring, NotificationImage.Credits, NotificationType.Standard, Colors.LightGreen, false, true, false);
           i++;
        }

        private void deleteCharButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (UI.SelectedChar != null)
            {
                int ind = TeraLogic.CharList.IndexOf(UI.SelectedChar);
                TeraLogic.DeletedChars.Add(TeraLogic.CharList[ind]);
                TeraLogic.CharList.Remove(UI.SelectedChar);
                UI.SelectedChar = null;
                UI.CharListContainer.chContainer.Items.RemoveAt(ind);
                TeraMainWindow.ExtendedCharacterStrips.Remove(TeraMainWindow.ExtendedCharacterStrips.Find(x => (string)x.Tag == TeraLogic.DeletedChars.Last().Name));

                UI.MainWin.undoButton.Opacity = 1;

                TeraLogic.UndoList.Add(new UndoDelegate(UndoDeletion));
            }
        }
        DoubleAnimation BarButtonFadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(70));
        DoubleAnimation BarButtonFadeOut = new DoubleAnimation(.3, TimeSpan.FromMilliseconds(120));

        private void BarButtonHoverIn(object sender, MouseEventArgs e)
        {
            (sender as System.Windows.Shapes.Rectangle).BeginAnimation(OpacityProperty, BarButtonFadeIn);
        }

        private void BarButtonHoverOut(object sender, MouseEventArgs e)
        {
            (sender as System.Windows.Shapes.Rectangle).BeginAnimation(OpacityProperty, BarButtonFadeOut);
        }


        private void resetWeeklyButtonClick(object sender, MouseButtonEventArgs e)
        {
            TeraLogic.UndoList.Add(new UndoDelegate(UndoWeeklyReset));
            foreach (var character in TeraLogic.CharList)
            {
                if (TeraLogic.ResettedWeekly.ContainsKey(character.Name))
                {
                    TeraLogic.ResettedWeekly.Remove(character.Name);
                }

                TeraLogic.ResettedWeekly.Add(character.Name, character.Weekly);
            }

            TeraLogic.UndoList.Add(new UndoDelegate(UndoWeeklyReset));
            TeraLogic.ResetWeeklyData();

        }
        private void resetDailyButtonClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var character in TeraLogic.CharList)
            {
                if (TeraLogic.ResettedDailies.ContainsKey(character.Name))
                {
                    TeraLogic.ResettedDailies.Remove(character.Name);
                }
                TeraLogic.ResettedDailies.Add(character.Name, character.Dailies);

            }

            TeraLogic.UndoList.Add(new UndoDelegate(UndoDailyReset));
            TeraLogic.ResetDailyData();
        }

        

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Activate();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UI.SendNotification(TeraLogic.CharList[0].GoldfingerTokens.ToString(), NotificationImage.Goldfinger, NotificationType.Counter, TCTData.Colors.FadedAccentColor, true, false, true);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UI.SendNotification(TeraLogic.CharList[0].GoldfingerTokens.ToString(), NotificationImage.Goldfinger, NotificationType.Standard, TCTData.Colors.SolidAccentColor, true, false, false);
        }
        private void TeraMainWin_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == WindowState.Minimized)
            //{
            //    this.ShowInTaskbar = false;
            //    //NI.Visible = true;
            //}
            //else if (this.WindowState == WindowState.Normal)
            //{
            //    //NI.Visible = false;
            //    this.ShowInTaskbar = true;
            //}
        }

        private void chViewButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var an = new DoubleAnimationUsingKeyFrames();

            main.ToggleDetails();
            if (main.DetailsExtended)
            {
                MinWidth = main.accountsColumn.MinWidth + main.chView.Width;
                var r = new RotateTransform(90);
                chViewButton.RenderTransform = r;
                if (isMinSize)
                {
                    Width = MinWidth;
                }
                else
                {
                    Width = accountsInitialWidth + main.chView.Width;
                }
            }
            else
            {
                var r = new RotateTransform(-90);
                chViewButton.RenderTransform = r;

                MinWidth = accountsInitialWidth + 20;
                if (isMinSize)
                {
                    Width = MinWidth;
                }
                else
                {
                    Width = MinWidth;
                }
            }

        }
        bool isMinSize = false;
        private void TeraMainWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(Width == MinWidth)
            {
                isMinSize = true;
            }
            else
            {
                isMinSize = false;
            }
        }
    }

}



