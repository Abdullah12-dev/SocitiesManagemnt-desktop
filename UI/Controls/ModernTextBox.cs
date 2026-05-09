using System.Runtime.InteropServices;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Controls;

public class ModernTextBox : Panel
{
    // Native Win32 cue-banner for single-line placeholder
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);
    private const uint EM_SETCUEBANNER = 0x1501;

    private TextBox? _inner;
    private string   _placeholder = "";
    private bool     _isFocused;
    private bool     _isMultiline;

    // Overlay label used for multiline placeholder (EM_SETCUEBANNER only works on single-line)
    private Label?   _placeholderLabel;

    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value;
            ApplyPlaceholder();
        }
    }

    public new string Text
    {
        get => _inner?.Text ?? "";
        set
        {
            if (_inner != null) _inner.Text = value;
        }
    }

    public bool UseSystemPasswordChar
    {
        get => _inner?.UseSystemPasswordChar ?? false;
        set { if (_inner != null) _inner.UseSystemPasswordChar = value; }
    }

    public bool Multiline
    {
        get => _isMultiline;
        set
        {
            _isMultiline = value;
            if (_inner != null)
            {
                _inner.Multiline = value;
                _inner.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
                LayoutInner();
            }
        }
    }

    public int MaxLength
    {
        get => _inner?.MaxLength ?? 0;
        set { if (_inner != null) _inner.MaxLength = value; }
    }

    public new event EventHandler? TextChanged
    {
        add    { if (_inner != null) _inner.TextChanged += value; }
        remove { if (_inner != null) _inner.TextChanged -= value; }
    }

    public ModernTextBox()
    {
        // Enable double-buffering to eliminate flicker / repaint-on-hover glitch
        SetStyle(
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint  |
            ControlStyles.ResizeRedraw,
            true);

        BackColor = AppTheme.InputBg;
        Cursor    = Cursors.IBeam;
        Height    = 40;

        _inner = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font        = AppTheme.FontInput,
            ForeColor   = AppTheme.TextPrimary,
            BackColor   = AppTheme.InputBg,
            Multiline   = false,
            TabStop     = true
        };

        _inner.GotFocus   += (_, _) => { _isFocused = true;  UpdatePlaceholderVisibility(); Invalidate(); };
        _inner.LostFocus  += (_, _) => { _isFocused = false; UpdatePlaceholderVisibility(); Invalidate(); };
        _inner.TextChanged += (_, _) => UpdatePlaceholderVisibility();

        Controls.Add(_inner);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyPlaceholder();
        LayoutInner();
    }

    private void ApplyPlaceholder()
    {
        if (_inner == null) return;

        if (!_isMultiline)
        {
            // Single-line: use native EM_SETCUEBANNER — works reliably without extra controls
            if (_inner.IsHandleCreated)
                SendMessage(_inner.Handle, EM_SETCUEBANNER, (IntPtr)1, _placeholder);
        }
        else
        {
            // Multiline: overlay a label (EM_SETCUEBANNER doesn't work on multiline)
            EnsurePlaceholderLabel();
            if (_placeholderLabel != null)
                _placeholderLabel.Text = _placeholder;
            UpdatePlaceholderVisibility();
        }
    }

    private void EnsurePlaceholderLabel()
    {
        if (_placeholderLabel != null) return;

        _placeholderLabel = new Label
        {
            Text      = _placeholder,
            Font      = AppTheme.FontInput,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent,
            AutoSize  = false,
            Location  = new Point(12, 8),
            Cursor    = Cursors.IBeam
        };

        _placeholderLabel.Click += (_, _) => _inner?.Focus();
        Controls.Add(_placeholderLabel);
        _placeholderLabel.BringToFront();
    }

    private void UpdatePlaceholderVisibility()
    {
        if (_placeholderLabel == null) return;
        _placeholderLabel.Visible = string.IsNullOrEmpty(_inner?.Text) && !_isFocused;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Draw border only — placeholder is handled natively or via label
        var color = _isFocused ? AppTheme.Accent : AppTheme.Border;
        float width = _isFocused ? 2f : 1f;
        using var pen = new Pen(color, width);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        e.Graphics.DrawRectangle(pen, rect);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutInner();
    }

    private void LayoutInner()
    {
        if (_inner == null) return;

        const int pad = 10;
        if (_isMultiline)
        {
            _inner.Location = new Point(pad, pad);
            _inner.Size     = new Size(Width - pad * 2, Height - pad * 2);
            if (_placeholderLabel != null)
                _placeholderLabel.Size = new Size(Width - pad * 2 - 4, Height - pad * 2);
        }
        else
        {
            int top = Math.Max(0, (Height - _inner.PreferredHeight) / 2);
            _inner.Location = new Point(pad, top);
            _inner.Size     = new Size(Width - pad * 2, _inner.PreferredHeight);
        }
    }
}
