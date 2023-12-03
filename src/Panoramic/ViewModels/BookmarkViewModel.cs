using System;
using System.Collections.Generic;
using System.Linq;
using Panoramic.Services;

namespace Panoramic.ViewModels;

public class BookmarkViewModel
{
    private const int TodayWeightMultiplier = 4;
    private const int PastWeekWeightMultiplier = 2;

    private readonly IEventHub _eventHub;

    public BookmarkViewModel(IEventHub eventHub, string title, Uri uri, List<DateTime> clicks)
    {
        _eventHub = eventHub;

        Id = uri.ToString();
        Title = title;
        Uri = uri;
        Clicks = clicks;
        LastClick = clicks.Last();
        
        CalculateWeight();
    }

    public string Id { get; set; }
    public string Title { get; set; }
    public Uri Uri { get; set; }
    public List<DateTime> Clicks { get; set; }
    public DateTime LastClick { get; set; }
    public int Weight { get; set; }

    public void Clicked()
    {
        LastClick = DateTime.Now;
        Clicks.Add(LastClick);
        CalculateWeight();

        _eventHub.RaiseHyperlinkClicked(Id, Title, Uri);
    }

    private void CalculateWeight()
    {
        var clicksToday = Clicks.Where(x => x.Date == DateTime.Today).Count();
        var clicksInThePastWeek = Clicks.Where(x => x.Date > DateTime.Now.AddDays(-7)).Count();
        var clicksInThePastMonth = Clicks.Where(x => x.Date < DateTime.Now.AddDays(-28)).Count();

        var todayWeight = clicksToday * 28 * TodayWeightMultiplier;
        var pastWeekWeight = clicksInThePastWeek * 4 * PastWeekWeightMultiplier;
        var pastMonthWeight = clicksInThePastMonth;

        Weight = todayWeight + pastWeekWeight + pastMonthWeight;
    }
}
