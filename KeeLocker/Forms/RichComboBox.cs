using System;

namespace KeeLocker.Forms
{
  class RichComboBox : System.Windows.Forms.ComboBox
  {
	public enum EItemType
	{
	  Active,
	  Inactive
	};

	public class SItem
	{
	  public string Text { get; set; }
	  public EItemType Type { get; set; }
	  public object Data { get; set; }

	  public SItem(string _Text, EItemType _Type, object _Data = null)
	  {
		Text = _Text;
		Data = _Data;
		Type = _Type;
	  }
      public override string ToString()
      {
		return Text;
      }
    };

	public System.Drawing.Color InactiveColor = System.Drawing.SystemColors.GrayText;
	public int ActiveShift = 20;
	private System.Drawing.Font InactiveFont;


	public RichComboBox()
	{
	  DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;

      FontChanged += RichComboBox_FontChanged;
      Disposed += RichComboBox_Disposed;
	  RichComboBox_FontChanged(null, EventArgs.Empty);
   }

	private void RichComboBox_Disposed(object sender, EventArgs e)
	{
	  if (InactiveFont != null)
		InactiveFont.Dispose();
	}

    private void RichComboBox_FontChanged(object sender, EventArgs e)
	{
	  if (InactiveFont != null)
		InactiveFont.Dispose();
	  InactiveFont = new System.Drawing.Font(Font.FontFamily, Font.Size, System.Drawing.FontStyle.Italic);
	}

    protected int LatestValidIndex = -1;

	public int Item_Add(SItem Item)
	{
	  if (LatestValidIndex == -1 && Item.Type == EItemType.Active)
	  {
		LatestValidIndex = Items.Count;
	  }

	  return Items.Add(Item);
	}

	public object GetDataForItem(object item)
	{
	  if (item == null)
		return null;
	  return ((SItem)item).Data;
	}

	protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
	{
	  if (e.Index < 0)
		return;
	  SItem item = (SItem)Items[e.Index];

	  EItemType Type = item.Type;
	  System.Drawing.Color Color;
	  bool ForceBg;
	  System.Drawing.Rectangle Bounds;
	  System.Drawing.Font Font;

	  if (Type == EItemType.Active)
	  {
		Color = e.ForeColor;
		ForceBg = false;
		Bounds = e.Bounds;
		Bounds.X += ActiveShift;
		Bounds.Width -= ActiveShift;
		Font = e.Font;
	  }
	  else // (Type == EItemType.Inactive)
	  {
		Color = InactiveColor;
		ForceBg = true;
		Bounds = e.Bounds;
		Font = InactiveFont;
	  }

	  e.DrawBackground();
	  if (ForceBg)
	  {
		using (var brush = new System.Drawing.SolidBrush(BackColor)) {
		  e.Graphics.FillRectangle(brush, e.Bounds);
		}
	  }
	  using (var brush = new System.Drawing.SolidBrush(Color)) {
		e.Graphics.DrawString(item.Text, Font, brush, Bounds);
	  }
	  if ((e.State & (System.Windows.Forms.DrawItemState.Focus | System.Windows.Forms.DrawItemState.NoFocusRect)) == System.Windows.Forms.DrawItemState.Focus)
	  {
		e.DrawFocusRectangle();
	  }
	}
	protected override void OnSelectedIndexChanged(EventArgs e)
	{
	  if (SelectedIndex != -1)
	  {
		SItem item = (SItem)Items[SelectedIndex];

		if (item.Type == EItemType.Inactive)
		{
		  SelectedIndex = LatestValidIndex;
		  return;
		}
		LatestValidIndex = SelectedIndex;
	  }
	  base.OnSelectedIndexChanged(e);
	}
  }
}
