using Embiggenator;
using UnityEngine;
using UnityEngine.UI;

public class AdaptablePreferred : LayoutElement, ILayoutElement
{
    [SerializeField] private RectTransform _contentToGrowWith;

    [SerializeField] private bool _usePreferredHeight;
    [SerializeField] private float _preferredHeightMax;

    private float _preferredHeight;

    public override float preferredHeight => _preferredHeight;

    public void Setup(RectTransform contentToGrowWith, bool usePreferredHeight, float preferredHeightMax)
    {
        _contentToGrowWith = contentToGrowWith;
        _usePreferredHeight = usePreferredHeight;
        _preferredHeightMax = preferredHeightMax;
    }

    public override void CalculateLayoutInputVertical()
    {
        if (_contentToGrowWith == null)
        {
            return;
        }
        if (_usePreferredHeight)
        {
            var height = LayoutUtility.GetPreferredHeight(_contentToGrowWith);
            _preferredHeight = _preferredHeightMax > height ? height : _preferredHeightMax;
        }
        else
        {
            _preferredHeight = -1;
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
    }
}