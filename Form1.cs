using System.Runtime.InteropServices;

namespace keycheck;

public partial class Form1 : Form
{
    private Dictionary<Keys, List<Button>> _keyButtons = new();

    public Form1()
    {
        InitializeComponent();
        this.Text = "Keyboard Tester";
        this.KeyPreview = true;
        this.StartPosition = FormStartPosition.CenterScreen;
        
        InitializeVirtualKeyboard();

        // Calculate optimal size based on layout
        // Last element is Numpad Enter/Plus column or Numpad 0 row
        // Numpad starts at navStartX + 170
        // navStartX is 835
        // numpadX = 835 + 170 = 1005
        // Rightmost edge = numpadX + 165 (column 4 start) + 50 (width) = 1005 + 215 = 1220
        // Add some padding
        int contentWidth = 1240;
        
        // Height: startY (20) + 6 rows * 55 = 20 + 330 = 350
        // Add padding
        int contentHeight = 380;

        this.ClientSize = new Size(contentWidth, contentHeight);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
    }

    private void InitializeVirtualKeyboard()
    {
        // Simple layout definition
        // Each item: (Key, Display Text, Width Multiplier)
        var layout = new List<List<(Keys Key, string Text, float Width)>>
        {
            // Row 1
            new() { 
                (Keys.Escape, "Esc", 1f), 
                (Keys.F1, "F1", 1f), (Keys.F2, "F2", 1f), (Keys.F3, "F3", 1f), (Keys.F4, "F4", 1f),
                (Keys.F5, "F5", 1f), (Keys.F6, "F6", 1f), (Keys.F7, "F7", 1f), (Keys.F8, "F8", 1f),
                (Keys.F9, "F9", 1f), (Keys.F10, "F10", 1f), (Keys.F11, "F11", 1f), (Keys.F12, "F12", 1f)
            },
            // Row 2
            new() {
                (Keys.Oem3, "`", 1f), (Keys.D1, "1", 1f), (Keys.D2, "2", 1f), (Keys.D3, "3", 1f), (Keys.D4, "4", 1f),
                (Keys.D5, "5", 1f), (Keys.D6, "6", 1f), (Keys.D7, "7", 1f), (Keys.D8, "8", 1f), (Keys.D9, "9", 1f),
                (Keys.D0, "0", 1f), (Keys.OemMinus, "-", 1f), (Keys.Oemplus, "=", 1f), (Keys.Back, "Backspace", 2f)
            },
            // Row 3
            new() {
                (Keys.Tab, "Tab", 1.5f), (Keys.Q, "Q", 1f), (Keys.W, "W", 1f), (Keys.E, "E", 1f), (Keys.R, "R", 1f),
                (Keys.T, "T", 1f), (Keys.Y, "Y", 1f), (Keys.U, "U", 1f), (Keys.I, "I", 1f), (Keys.O, "O", 1f),
                (Keys.P, "P", 1f), (Keys.OemOpenBrackets, "[", 1f), (Keys.OemCloseBrackets, "]", 1f), (Keys.Oem5, "\\", 1.5f)
            },
            // Row 4
            new() {
                (Keys.CapsLock, "Caps", 1.75f), (Keys.A, "A", 1f), (Keys.S, "S", 1f), (Keys.D, "D", 1f), (Keys.F, "F", 1f),
                (Keys.G, "G", 1f), (Keys.H, "H", 1f), (Keys.J, "J", 1f), (Keys.K, "K", 1f), (Keys.L, "L", 1f),
                (Keys.Oem1, ";", 1f), (Keys.Oem7, "'", 1f), (Keys.Enter, "Enter", 2.25f)
            },
            // Row 5
            new() {
                (Keys.LShiftKey, "Shift", 2.25f), (Keys.Z, "Z", 1f), (Keys.X, "X", 1f), (Keys.C, "C", 1f), (Keys.V, "V", 1f),
                (Keys.B, "B", 1f), (Keys.N, "N", 1f), (Keys.M, "M", 1f), (Keys.Oemcomma, ",", 1f), (Keys.OemPeriod, ".", 1f),
                (Keys.OemQuestion, "/", 1f), (Keys.RShiftKey, "Shift", 2.75f)
            },
            // Row 6
            new() {
                (Keys.LControlKey, "Ctrl", 1.25f), (Keys.LWin, "Win", 1.25f), (Keys.LMenu, "Alt", 1.25f),
                (Keys.Space, "Space", 6.25f),
                (Keys.RMenu, "Alt", 1.25f), (Keys.RWin, "Win", 1.25f), (Keys.Apps, "Menu", 1.25f), (Keys.RControlKey, "Ctrl", 1.25f)
            }
        };

        int startX = 20;
        int startY = 20;
        int keySize = 50;
        int gap = 5;

        int currentY = startY;
        int targetRightEdge = 0; // The right X coordinate of the standard row (Row 2)

        for (int r = 0; r < layout.Count; r++)
        {
            var row = layout[r];
            int currentX = startX;
            Button? lastBtn = null;

            foreach (var key in row)
            {
                int width = (int)(keySize * key.Width);
                var btn = new Button
                {
                    Text = key.Text,
                    Tag = key.Key,
                    Size = new Size(width, keySize),
                    Location = new Point(currentX, currentY),
                    Enabled = true,
                    TabStop = false,
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Store reference
                // Note: Duplicate keys (like Shift) need handling. 
                // We'll store both if we can, or just handle generic keys if needed.
                // For simplicity, let's map by key code. 
                // Since Dictionary keys must be unique, we need to handle Left/Right properly.
                // The Keys enum has LShiftKey, RShiftKey, etc. so they are unique.
                
                if (!_keyButtons.ContainsKey(key.Key))
                {
                    _keyButtons.Add(key.Key, new List<Button> { btn });
                }
                else
                {
                    _keyButtons[key.Key].Add(btn);
                }

                this.Controls.Add(btn);
                currentX += width + gap;

                // Add grouping gaps for Function Keys row
                if (r == 0)
                {
                    if (key.Key == Keys.Escape || key.Key == Keys.F4 || key.Key == Keys.F8)
                    {
                        // Calculate extra gap to align F12 with Backspace
                        // Standard Row 2 Width (approx) = 15 units + 13 gaps
                        // Standard Row 1 Width (approx) = 13 units + 12 gaps
                        // Difference = 2 units + 1 gap
                        // We have 3 split points (Esc-F1, F4-F5, F8-F9)
                        // So each split needs (2*unit + gap) / 3 extra space
                        
                        int extraGap = (int)((2 * keySize + gap) / 3.0f);
                        currentX += extraGap;
                    }
                }

                lastBtn = btn;
            }

            // Align Row 3, 4, 5, 6 to Row 2
            if (r == 1) // Row 2 (Number row) is our reference
            {
                targetRightEdge = currentX - gap;
            }
            else if (r > 1 && lastBtn != null) // Main block rows
            {
                int currentRightEdge = currentX - gap;
                int diff = targetRightEdge - currentRightEdge;
                if (diff != 0)
                {
                    lastBtn.Width += diff;
                }
            }

            currentY += keySize + gap;
        }
        
        // Add navigation keys block separately
        int navStartX = 845; // Moved left from 880 to reduce gap
        int navY = startY;

        // Helper to add button
        void AddButton(Keys k, string t, int x, int y, int w = 50)
        {
             var btn = new Button
             {
                 Text = t,
                 Tag = k,
                 Size = new Size(w, keySize),
                 Location = new Point(x, y),
                 Enabled = true,
                 TabStop = false,
                 BackColor = Color.White,
                 ForeColor = Color.Black,
                 Font = new Font("Segoe UI", 8, FontStyle.Bold)
             };
             if (!_keyButtons.ContainsKey(k)) _keyButtons.Add(k, new List<Button> { btn });
             else _keyButtons[k].Add(btn);
             this.Controls.Add(btn);
        }

        // Row 1: PrtSc, Scroll, Pause
        AddButton(Keys.PrintScreen, "PrtSc", navStartX, navY);
        AddButton(Keys.Scroll, "ScrlLk", navStartX + 55, navY);
        AddButton(Keys.Pause, "Pause", navStartX + 110, navY);

        // Row 2: Ins, Home, PgUp
        navY += 55;
        AddButton(Keys.Insert, "Ins", navStartX, navY);
        AddButton(Keys.Home, "Home", navStartX + 55, navY);
        AddButton(Keys.PageUp, "PgUp", navStartX + 110, navY);

        // Row 3: Del, End, PgDn
        navY += 55;
        AddButton(Keys.Delete, "Del", navStartX, navY);
        AddButton(Keys.End, "End", navStartX + 55, navY);
        AddButton(Keys.PageDown, "PgDn", navStartX + 110, navY);

        // Arrows
        // Up is usually in the gap between Row 4 and Row 5 logic-wise, but physically:
        // Row 5 (Shift) Y is startY + 4 * 55 = 20 + 220 = 240.
        // Arrow Up is usually aligned with RShift roughly.
        // Let's put Arrows at the bottom right.
        
        int arrowY = startY + 4 * 55; // Row 5 level
        // Up
        AddButton(Keys.Up, "▲", navStartX + 55, arrowY);
        
        arrowY += 55; // Row 6 level
        // Left, Down, Right
        AddButton(Keys.Left, "◄", navStartX, arrowY);
        AddButton(Keys.Down, "▼", navStartX + 55, arrowY);
        AddButton(Keys.Right, "►", navStartX + 110, arrowY);

        // Numpad Area
        int numpadX = navStartX + 170; // Offset from nav area
        int numpadY = startY + 55; // Align with Row 2 (Ins/Home/PgUp level usually) - actually standard numpad starts at row 2

        // Row 1: NumLock, /, *, -
        AddButton(Keys.NumLock, "Num", numpadX, numpadY);
        AddButton(Keys.Divide, "/", numpadX + 55, numpadY);
        AddButton(Keys.Multiply, "*", numpadX + 110, numpadY);
        AddButton(Keys.Subtract, "-", numpadX + 165, numpadY);

        // Row 2: 7, 8, 9, + (tall)
        numpadY += 55;
        AddButton(Keys.NumPad7, "7", numpadX, numpadY);
        AddButton(Keys.NumPad8, "8", numpadX + 55, numpadY);
        AddButton(Keys.NumPad9, "9", numpadX + 110, numpadY);
        
        // Plus key spans 2 rows
        var plusBtn = new Button
        {
            Text = "+",
            Tag = Keys.Add,
            Size = new Size(50, keySize * 2 + gap),
            Location = new Point(numpadX + 165, numpadY),
            Enabled = true,
            TabStop = false,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };
        if (!_keyButtons.ContainsKey(Keys.Add)) _keyButtons.Add(Keys.Add, new List<Button> { plusBtn });
        else _keyButtons[Keys.Add].Add(plusBtn);
        this.Controls.Add(plusBtn);

        // Row 3: 4, 5, 6
        numpadY += 55;
        AddButton(Keys.NumPad4, "4", numpadX, numpadY);
        AddButton(Keys.NumPad5, "5", numpadX + 55, numpadY);
        AddButton(Keys.NumPad6, "6", numpadX + 110, numpadY);

        // Row 4: 1, 2, 3, Enter (tall)
        numpadY += 55;
        AddButton(Keys.NumPad1, "1", numpadX, numpadY);
        AddButton(Keys.NumPad2, "2", numpadX + 55, numpadY);
        AddButton(Keys.NumPad3, "3", numpadX + 110, numpadY);

        // Enter key spans 2 rows
        // Note: Keys.Enter is usually the main enter. Numpad Enter often shares the same code or is distinguished by extended bit.
        // WinForms Keys enum doesn't always distinguish Numpad Enter clearly from Main Enter without checking modifiers/lparam.
        // But for visual purposes, let's create a button. We might map it to a custom tag or just Keys.Enter and handle overlap logic.
        // However, standard Keys.Enter triggers the main one. 
        // We will assign a special dummy key or check if we can distinguish.
        // For now, let's just use a separate visual button, but link it to Keys.Return if possible or handle distinction.
        // Actually, if we use the same key code, both will highlight. That's acceptable behavior for "Enter".
        // But wait, user might want to test if Numpad Enter works specifically.
        // Let's create it and see. If they share Keys.Enter, both highlight.
        
        // We'll use a trick: If we want them separate, we need low-level hooks. 
        // But for this simple app, highlighting both is fine, OR we can try to use the fact that we have two buttons for one key code.
        // Our HighlightKey looks up _keyButtons by Key.
        // If we want two buttons for one Key, we need a list of buttons per key.
        
        // Let's first add the button.
        var numEnterBtn = new Button
        {
            Text = "Ent",
            Tag = Keys.Enter, // Map to Keys.Enter so it highlights together with main Enter
            Size = new Size(50, keySize * 2 + gap),
            Location = new Point(numpadX + 165, numpadY),
            Enabled = true,
            TabStop = false,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };
        if (!_keyButtons.ContainsKey(Keys.Enter)) _keyButtons.Add(Keys.Enter, new List<Button> { numEnterBtn });
        else _keyButtons[Keys.Enter].Add(numEnterBtn);
        this.Controls.Add(numEnterBtn);

        // Row 5: 0 (wide), .
        numpadY += 55;
        AddButton(Keys.NumPad0, "0", numpadX, numpadY, 105); // Width 50*2 + 5 = 105
        AddButton(Keys.Decimal, ".", numpadX + 110, numpadY);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HighlightKey(e.KeyCode, true);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        HighlightKey(e.KeyCode, false);
    }

    private void HighlightKey(Keys key, bool isPressed)
    {
        if (_keyButtons.TryGetValue(key, out var btns))
        {
            foreach (var btn in btns)
            {
                btn.BackColor = isPressed ? Color.LightGreen : Color.White;
            }
        }
        else
        {
            // Handle generic keys -> highlight both or specific ones if possible
            if (key == Keys.ShiftKey)
            {
                HighlightKey(Keys.LShiftKey, isPressed);
                HighlightKey(Keys.RShiftKey, isPressed);
            }
            else if (key == Keys.ControlKey)
            {
                HighlightKey(Keys.LControlKey, isPressed);
                HighlightKey(Keys.RControlKey, isPressed);
            }
            else if (key == Keys.Menu) // Alt
            {
                HighlightKey(Keys.LMenu, isPressed);
                HighlightKey(Keys.RMenu, isPressed);
            }
        }
    }
}
