using QI_CMS_v2.Utils;
using QI_CMS_v2.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static QI_CMS_v2.Models.PlayableItem;
using static QI_CMS_v2.Models.Schedule;
using static QI_CMS_v2.Utils.DateUtils;

namespace QI_CMS_v2.Models
{
    public class ActualScheduleMedia
    {
        public class MediaBase
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }

        public class ImageInfo : MediaBase
        {
            public string Path { get; set; }
        }

        public class VideoInfo : MediaBase
        {
            public string Path { get; set; }
        }

        public class TextInfo : MediaBase
        {
            public string Text { get; set; }
            public string BackgroundColor { get; set; }
            public string FontColor { get; set; }
            public int FontSize { get; set; }
            public string FontFamily { get; set; }

        }
    }

    public class ActualScheduleItem
    {
        public DateTime Start { get; set; }
        public TimeSpan StartPosition { get; set; } = TimeSpan.Zero;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public TimeSpan ActualDuration { get => Duration - StartPosition; }
        public DateTime End { get => Start + ActualDuration; }

        public MediaContentType Type { get; set; }
        public MediaContent Content { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string BackgroundColor { get; set; } = "#000000";

        public ActualScheduleItem Clone()
        {
            return new ActualScheduleItem()
            {
                Start = Start,
                StartPosition = StartPosition,
                Duration = Duration,
                Type = Type,
                Content = Content,
                X = X, Y = Y,
                Width = Width, Height = Height,
                BackgroundColor = BackgroundColor,
            };
        }
    }

    public class ActualSchedule
    {
        public List<ActualScheduleItem> Items = [];
        public DateTime Start { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime End { get => Start + Duration; }
        public int Priority { get; set; }

        public ActualSchedule() { }

        public ActualSchedule Clone()
        {
            return new ActualSchedule()
            {
                Start = Start,
                Items = [..Items],
                Duration = Duration,
                Priority = Priority
            };
        }

        public void AddScheduleItems(Schedule schedule, Size size)
        {
            var totalDuration = TimeSpan.FromSeconds(schedule.Duration);

            foreach (var playable in schedule.Items)
            {
                double x = playable.EvaluateX(size);
                double y = playable.EvaluateY(size);
                double width = playable.EvaluateWidth(size);
                double height = playable.EvaluateHeight(size);
                var duration = TimeSpan.FromSeconds(playable.Duration);

                // 유저가 직접 설정한 재생 시간에 맞추기 위해 뒤의 재생 아이템을 잘라냄
                if (totalDuration > TimeSpan.Zero)
                {
                    duration = totalDuration > duration ? duration : totalDuration;

                    var item = new ActualScheduleItem()
                    {
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height,
                        Duration = duration,
                        Type = playable.ContentType,
                        Content = playable.Content,
                        BackgroundColor = playable.BackgroundColor
                    };

                    Items.Add(item);
                }
                else break;
            }
        }

        // 스케줄의 시작 시점을 뒤로 미루는 메소드
        // 미뤄진 시간 이전에 종료되는 아이템은 제거되고, 미뤄진 시간에 시작돼야할 아이템이 시작되는 시점을 재계산함
        public void DelayStartTime(DateTime start)
        {
            if (start < Start)
            {
                throw new ArgumentException("원래 스케줄의 시작 시간보다 일찍 시작할 수 없습니다.");
            }
            if (start >= End)
            {
                throw new ArgumentException("원래 스케줄의 종료 시간보다 늦게 시작할 수 없습니다.");
            }

            var skipTime = start - Start;
            Duration -= skipTime;

            var itemCount = Items.Count;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (skipTime >= item.Duration)
                {
                    skipTime -= item.Duration;
                    Items.RemoveAt(i);
                    itemCount--;
                }
                else
                {
                    item.StartPosition = skipTime;
                    break;
                }
            }

            Start = start;
        }
        
        // 스케줄의 종료 시점을 앞당기는 메소드
        // 앞당겨진 시간 이후에 시작하는 아이템은 제거되고, 앞당겨진 시간에 종료돼야할 아이템의 재생 시간을 재계산함
        public void BringForwardEndTime(DateTime end)
        {
            if (End < end)
            {
                throw new ArgumentException("원래 스케줄의 종료 시간보다 늦게 종료할 수 없습니다.");
            }
            if (Start >= end)
            {
                throw new ArgumentException("원래 스케줄의 시작 시간보다 일찍 종료할 수 없습니다.");
            }

            var removalTime = End - end;
            var itemCount = Items.Count;

            for (var i = itemCount - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (removalTime >= item.Duration)
                {
                    removalTime -= item.Duration;
                    Items.RemoveAt(i);
                }
                else
                {
                    item.Duration -= removalTime;
                    break;
                }
            }

            Duration = end - Start;
        }

        public List<ActualScheduleItem> GetActualScheduleItems()
        {
            var items = new List<ActualScheduleItem>();
            var start = Start;
            foreach (var item in Items)
            {
                var newItem = item.Clone();
                newItem.Start = start;
                start += newItem.ActualDuration;
                items.Add(newItem);
            }

            return items;
        }
    }

    public class LayerScheduler
    {

        private TimeSpan _interval;
        private DateTime _lastEndTime;
        private Task _scheduleTask;
        private readonly Func<CancellationToken, Task> _scheduleAction;
        private Mutex _scheduleLock = new Mutex();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;

        public DisplayLayer Layer;
        public List<ActualSchedule> Schedules = [];

        private List<ActualSchedule> _defaultSchedules; // Start 시간이 정의 안된 Default 타입 스케줄 리스트
        private int _lastDefaultScheduleIndex;

        private MediaPlayerWindow _player;

        public LayerScheduler(DisplayLayer layer, DateTime start, TimeSpan interval)
        {
            Layer = layer;
            _interval = interval;
            _lastEndTime = start + interval;

            _scheduleAction = async (token) =>
            {
                // 여기에 주기적으로 실행할 작업 구현
                await Task.Run(() =>
                {
                    GetNextTimeTable();
                }, token);
            };

            // Default Schedule 입력
            _defaultSchedules = new List<ActualSchedule>();
            _lastDefaultScheduleIndex = 0;
            foreach (var schedule in Layer.DefaultSchedules)
            {
                var duration = TimeSpan.FromSeconds(schedule.Duration);
                var priority = schedule.GetSchedulePriority();
                var actualSchedule = new ActualSchedule()
                {
                    Duration = duration,
                    Priority = priority
                };
                actualSchedule.AddScheduleItems(schedule, Layer.Bounds.Size);
                _defaultSchedules.Add(actualSchedule);
            }

            Schedules = GetTimeTableWithin(start, _interval, null);
            ReserveSchedulingJob().Wait();
            Play();
        }


        private void Play()
        {
            _player = new MediaPlayerWindow(this);
            Enqueue(5);
            _player.Play();
            _player.Show();
        }

        private async Task ReserveSchedulingJob()
        {
            if (_isRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            _scheduleTask = Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await _scheduleAction(_cancellationTokenSource.Token);
                        await Task.Delay(_interval, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 취소는 정상적인 종료이므로 무시
                }
                catch (Exception ex)
                {
                    // 오류 처리
                    Console.WriteLine($"Background work error: {ex.Message}");
                }
                finally
                {
                    _isRunning = false;
                }
            }, _cancellationTokenSource.Token);
        }

        public async void Stop()
        {
            if (!_isRunning) return;

            _cancellationTokenSource.Cancel();

            Dispose();
        }

        public void Dispose()
        {
            _player.Dispose();
            _cancellationTokenSource.Dispose();
            _scheduleLock.Dispose();
        }

        public void Enqueue(int size)
        {
            _scheduleLock.WaitOne();
            var totalItems = new List<ActualScheduleItem>();
            int enqMax = Math.Min(size, Schedules.Count);
            for (int i = 0; i < enqMax; i++)
            {
                var schedule = Schedules.First();
                var items = schedule.GetActualScheduleItems();
                totalItems.AddRange(items);
                Schedules.Remove(schedule);
            }
            _scheduleLock.ReleaseMutex();
            _player.Enqueue(totalItems);
        }

        private void GetNextTimeTable()
        {
            _scheduleLock.WaitOne();
            Schedules = GetTimeTableWithin(_lastEndTime, _interval, Schedules);
            for (int s = 0; s < Schedules.Count; s++)
            {
                var t = Schedules[s];
                Debug.WriteLine($"{Layer.Name}: Schedule{s}[{t.Start}, {t.End}, {t.Priority}]");
            }
            _lastEndTime += _interval;
            _scheduleLock.ReleaseMutex();
        }

        // Repeat 스케줄의 start 이후 첫번째 schedule 재생 시간 구하기
        public static DateTime GetFirstDateTimeOfRepeatScheduleAfter(Schedule schedule, DateTime start)
        {
            if (schedule.Type != Schedule.ScheduleType.Repeat)
            {
                throw new ArgumentException("올바른 Repeat 타입 스케줄이 아닙니다.");
            }

            var scheduleTimeOnly = schedule.StartTime;
            var startTimeOnly = TimeOnly.FromDateTime(start);

            if (schedule.Period == Schedule.RepeatPeriod.Day)
            {
                var firstScheduleDateTime = start.Date.Add(scheduleTimeOnly.ToTimeSpan());
                if (firstScheduleDateTime < start) firstScheduleDateTime = firstScheduleDateTime.AddDays(1);

                return firstScheduleDateTime;
            }
            else if (schedule.Period == Schedule.RepeatPeriod.Week)
            {
                var scheduleDayOfWeek = schedule.PeriodDayOfWeek;
                var startDayOfWeek = start.DayOfWeek;

                var firstScheduleDayOfWeekGap = scheduleDayOfWeek - startDayOfWeek;
                if (firstScheduleDayOfWeekGap < 0) firstScheduleDayOfWeekGap += 7;
                var firstScheduleDate = start.Date.AddDays(firstScheduleDayOfWeekGap);
                var firstScheduleDateTime = firstScheduleDate.Add(scheduleTimeOnly.ToTimeSpan());
                if (firstScheduleDateTime < start) firstScheduleDateTime = firstScheduleDateTime.AddDays(7);

                return firstScheduleDateTime;
            }
            else if (schedule.Period == Schedule.RepeatPeriod.Month)
            {
                int scheduleDay = schedule.PeriodDay;
                int firstYear = start.Year;
                int firstMonth = start.Month;
                while (true)
                {
                    if (scheduleDay <= DateTime.DaysInMonth(firstYear, firstMonth))
                    {
                        var firstScheduleDateTime = new DateTime(firstYear, firstMonth, scheduleDay, scheduleTimeOnly.Hour, scheduleTimeOnly.Minute, scheduleTimeOnly.Second);
                        if (firstScheduleDateTime >= start) return firstScheduleDateTime;
                    }
                    firstMonth = firstMonth + 1;
                    if (firstMonth > 12)
                    {
                        firstMonth = 1;
                        firstYear++;
                    }
                }
            }
            else if (schedule.Period == Schedule.RepeatPeriod.Year)
            {
                int scheduleDay = schedule.PeriodDay;
                int scheduleMonth = schedule.PeriodMonth;
                int firstYear = start.Year;
                while (true)
                {
                    if (scheduleDay <= DateTime.DaysInMonth(firstYear, scheduleMonth))
                    {
                        var firstScheduleDateTime = new DateTime(firstYear, scheduleMonth, scheduleDay, scheduleTimeOnly.Hour, scheduleTimeOnly.Minute, scheduleTimeOnly.Second);
                        if (firstScheduleDateTime >= start) return firstScheduleDateTime;
                    }
                    firstYear = firstYear + 1;
                }
            }
            throw new ArgumentException("올바른 Repeat 타입 스케줄이 아닙니다.");
        }


        /* 
         * 정렬 트리를 기반으로 스케줄 리스트 새로 생성
         * 입력 트리 상태: 시작 시간이 빠른 순서 + 시작 시간이 같고 종료시간이 늦는 순서로 정렬
         * 루프 돌면서 직전 스케줄과 현재 스케줄 비교
         * 
         * !!!!!!!!!!!!!! 구현 유의사항 !!!!!!!!!!!!!!!
         * 앞서 트리에서 중복을 제거했더라도, 리스트 생성 도중에 현재 스케줄이 직전 스케줄과 s,e 모두 같아지는 경우가 있을 수 있음
         * 직전 스케줄이 그 전 스케줄에 의해 s가 미뤄져서 현재 스케줄의 s와 같아질 수 있기 때문
         * 
         * 직전과 구간 안 겹치면 바로 삽입
         * 겹치면 경우에 따라 처리
         * 
         * 겹칠 때 경우의 수
         * Priority : 직전이 더 높을 때, 현재가 더 높을 때, 같을 때
         * Start : 동시에 시작할 때, 현재가 더 늦게 시작할 때
         * End : 동시에 끝날 때, 직전이 더 일찍 끝날 때, 현재가 더 일찍 끝날 때
         * 
         * Priority 기본 원칙
         * Priority가 더 높은 스케줄은 낮은 스케줄의 재생 시간을 차지할 수 있음. 같을 경우, 더 빨리 시작한 스케줄이 우선 순위를 가지게 됨.
         */
        private List<ActualSchedule> GetSortedScheduleList(DateTimeSpanTree<ActualSchedule> scheduleTree)
        {
            var scheduleList = new List<ActualSchedule>();
            if (scheduleTree.Count > 0)
            {
                var scheduleTreeList = scheduleTree.GetAll().ToList();

                // 시작 스케줄 삽입
                var firstActualSchedule = scheduleTreeList.First().Value;
                scheduleList.Add(firstActualSchedule);

                for (int i = 1; i < scheduleTreeList.Count; i++)
                {
                    var pair = scheduleTreeList[i];
                    var curActualSchedule = pair.Value;
                    var prevActualSchedule = scheduleList.Last();

                    var prevStart = prevActualSchedule.Start;
                    var prevEnd = prevActualSchedule.End;
                    var prevPriority = prevActualSchedule.Priority;

                    var curStart = curActualSchedule.Start;
                    var curEnd = curActualSchedule.End;
                    var curPriority = curActualSchedule.Priority;

                    // 직전 스케줄과 재생 시간이 겹치지 않을 때
                    if (prevEnd <= curStart)
                    {
                        scheduleList.Add(curActualSchedule);
                    }
                    // 겹칠 때
                    else
                    {
                        // 동시에 시작하고, 동시에 끝날 때 : 직전 Priority가 더 낮으면, 직전 삭제 & 현재 추가. 높거나 같으면, 현재 추가 X.
                        // 동시에 시작하고, 직전이 더 일찍 끝날 때 : 직전 Priority가 더 낮으면, 직전의 종료를 현재의 시작으로 당기고 현재 추가. 높거나 같으면, 현재의 시작을 직전 종료로 미루고 현재 추가.
                        // 동시에 시작하고, 현재가 더 일찍 끝날 때 : 직전 Priority가 더 낮으면, 현재를 추가 후 직전의 시작을 현재 종료로 미루고 현재 뒤로 이동. 높거나 같으면, 현재 추가 X.
                        if (prevStart == curStart)
                        {
                            if (prevEnd == curEnd)
                            {
                                if (prevPriority < curPriority)
                                {
                                    scheduleList.Remove(prevActualSchedule);
                                    scheduleList.Add(curActualSchedule);
                                }
                            }
                            else if (prevEnd < curEnd)
                            {
                                if (prevPriority < curPriority)
                                {
                                    prevActualSchedule.BringForwardEndTime(curStart);
                                    scheduleList.Add(curActualSchedule);
                                }
                                else
                                {
                                    curActualSchedule.DelayStartTime(prevEnd);
                                    scheduleList.Add(curActualSchedule);
                                }
                            }
                            else
                            {
                                if (prevPriority < curPriority)
                                {
                                    scheduleList.Remove(prevActualSchedule);
                                    scheduleList.Add(curActualSchedule);
                                    prevActualSchedule.DelayStartTime(curEnd);
                                    scheduleList.Add(prevActualSchedule);
                                }
                            }
                        }
                        // 현재가 더 늦게 시작하고, 동시에 끝날 때 : 직전 Priority가 더 낮으면, 직전의 종료를 현재 시작으로 당기고 현재 추가. 높거나 같으면, 현재 추가 X.
                        // 현재가 더 늦게 시작하고, 직전이 더 일찍 끝날 때 : 직전 Priority가 더 낮으면, 직전의 종료를 현재의 시작으로 당기고 현재 추가. 높거나 같으면, 현재의 시작을 직전 종료로 미루고 현재 추가.
                        // 현재가 더 늦게 시작하고, 현재가 더 일찍 끝날 때 : 직전 Priority가 더 낮으면, 직전을 둘로 분할해서 현재를 사이에 추가. 높거나 같으면, 현재 추가 X.
                        else
                        {
                            if (prevEnd == curEnd)
                            {
                                if (prevPriority < curPriority)
                                {
                                    prevActualSchedule.BringForwardEndTime(curStart);
                                    scheduleList.Add(curActualSchedule);
                                }
                            }
                            else if (prevEnd < curEnd)
                            {
                                if (prevPriority < curPriority)
                                {
                                    prevActualSchedule.BringForwardEndTime(curStart);
                                    scheduleList.Add(curActualSchedule);
                                }
                                else
                                {
                                    curActualSchedule.DelayStartTime(prevEnd);
                                    scheduleList.Add(curActualSchedule);
                                }
                            }
                            else
                            {
                                if (prevPriority < curPriority)
                                {
                                    var nextActualSchedule = prevActualSchedule.Clone();
                                    prevActualSchedule.BringForwardEndTime(curStart);
                                    nextActualSchedule.DelayStartTime(curEnd);
                                    scheduleList.Add(curActualSchedule);
                                    scheduleList.Add(nextActualSchedule);
                                }
                            }
                        }
                    }
                }
            }

            return scheduleList;
        }

        private List<ActualSchedule> GetDefaultScheduleListBetween(DateTime start, DateTime end)
        {
            var defaultScheduleList = new List<ActualSchedule>();

            var count = _defaultSchedules.Count;
            if (count > 0)
            {
                while (true)
                {
                    var curDefaultSchedule = _defaultSchedules[_lastDefaultScheduleIndex];
                    if (start + curDefaultSchedule.Duration <= end)
                    {
                        curDefaultSchedule.Start = start;
                        defaultScheduleList.Add(curDefaultSchedule.Clone());
                        _lastDefaultScheduleIndex = _lastDefaultScheduleIndex + 1 < count ? _lastDefaultScheduleIndex + 1 : 0;
                        start = start + curDefaultSchedule.Duration;
                    }
                    else
                    {
                        if (start < end)
                        {
                            curDefaultSchedule.Start = start;
                            curDefaultSchedule = curDefaultSchedule.Clone();
                            defaultScheduleList.Add(curDefaultSchedule);
                            _lastDefaultScheduleIndex = _lastDefaultScheduleIndex + 1 < count ? _lastDefaultScheduleIndex + 1 : 0;
                        }
                        break;
                    }
                }
            }

            return defaultScheduleList;
        }

        private List<ActualSchedule> MergeScheduleList(List<ActualSchedule> before, List<ActualSchedule> after)
        {
            if (before.Count > 0)
            {
                for (int i = 0; i < after.Count; i++)
                {
                    var curSchedule = after[i];
                    var lastActualSchedule = before.Last();
                    if (lastActualSchedule.End > curSchedule.Start)
                    {
                        if (lastActualSchedule.End > curSchedule.End)
                        {
                            if (lastActualSchedule.Priority >= curSchedule.Priority)
                            {
                                after.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                var nextSchedule = lastActualSchedule.Clone();
                                lastActualSchedule.BringForwardEndTime(curSchedule.Start);
                                nextSchedule.DelayStartTime(curSchedule.End);
                                
                                before.Add(curSchedule);
                                before.Add(nextSchedule);

                                after.RemoveAt(i);
                                i--;
                            }
                        }
                        else if (lastActualSchedule.End < curSchedule.End)
                        {
                            if (lastActualSchedule.Priority >= curSchedule.Priority)
                            {
                                curSchedule.DelayStartTime(lastActualSchedule.End);
                            }
                            else
                            {
                                lastActualSchedule.BringForwardEndTime(curSchedule.Start);
                            }
                            break;
                        }
                        else
                        {
                            if (lastActualSchedule.Priority >= curSchedule.Priority)
                            {
                                after.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                lastActualSchedule.BringForwardEndTime(curSchedule.Start);
                            }
                            break;
                        }
                    }
                }
                before.AddRange(after);
                return before;
            }
            else return after;
        }

        // [start, start + interval) 사이에 시작하는 Schedule을 계산하여 리스트 반환
        public List<ActualSchedule> GetTimeTableWithin(DateTime start, TimeSpan interval, List<ActualSchedule>? before = null)
        {
            var curScheduleList = before ?? [];
            var scheduleTree = new DateTimeSpanTree<ActualSchedule>();

            var end = start + interval;

            // Disposable 스케줄 우선 삽입
            foreach (var schedule in Layer.DisposableSchedules)
            {
                var scheduleStart = schedule.StartDateTime;
                var duration = TimeSpan.FromSeconds(schedule.Duration);
                var priority = schedule.GetSchedulePriority();

                // 사이에 시작하는지 확인
                if (scheduleStart >= start && scheduleStart < end)
                {
                    // 이미 있으면 스킵
                    if (scheduleTree.TryGetValue(scheduleStart, duration, out var actualSchedule))
                    {
                        continue;
                    }

                    // 아닐 경우 삽입
                    actualSchedule = new ActualSchedule()
                    {
                        Start = scheduleStart,
                        Duration = duration,
                        Priority = priority,
                    };
                    actualSchedule.AddScheduleItems(schedule, Layer.Bounds.Size);

                    scheduleTree.Add(scheduleStart, duration, actualSchedule);
                }
            }

            // Repeat 스케줄 삽입
            foreach (var schedule in Layer.RepeatSchedules)
            {
                try
                {
                    var scheduleStart = GetFirstDateTimeOfRepeatScheduleAfter(schedule, start);
                    var duration = TimeSpan.FromSeconds(schedule.Duration);
                    var priority = schedule.GetSchedulePriority();

                    // 사이에 시작하는지 확인
                    if (scheduleStart >= start && scheduleStart < end)
                    {
                        // 이미 있을 때 Priority가 더 높으면 스킵
                        if (scheduleTree.TryGetValue(scheduleStart, duration, out var actualSchedule))
                        {
                            if (actualSchedule.Priority >= priority)
                            {
                                continue;
                            }
                        }

                        // 아닐 경우 삽입
                        actualSchedule = new ActualSchedule()
                        {
                            Start = scheduleStart,
                            Duration = duration,
                            Priority = priority,
                        };
                        actualSchedule.AddScheduleItems(schedule, Layer.Bounds.Size);

                        scheduleTree.Add(scheduleStart, duration, actualSchedule);
                    }
                }
                catch 
                {
                    
                }
            }

            var nextScheduleList = GetSortedScheduleList(scheduleTree);
            var mergedScheduleList = MergeScheduleList(curScheduleList, nextScheduleList);

            // 비어 있는 시간에 Default 스케줄 삽입
            if (Layer.DefaultSchedules.Count > 0)
            {
                // Todo: nextScheduleList와 현재 스케줄 리스트 merge
                if (curScheduleList.Count > 0)
                {
                    var startIndex = curScheduleList.Count - 1;
                    if (nextScheduleList.Count > 0)
                    {
                        do
                        {
                            var defaultStart = mergedScheduleList[startIndex].End;
                            var defaultEnd = startIndex + 1 >= mergedScheduleList.Count ? end : mergedScheduleList[startIndex + 1].Start;
                            var defaultScheduleList = GetDefaultScheduleListBetween(defaultStart, defaultEnd);
                            if (defaultEnd < end && defaultScheduleList.Count > 0)
                            {
                                defaultScheduleList.Last().BringForwardEndTime(end);
                            }
                            mergedScheduleList.InsertRange(startIndex, defaultScheduleList);
                            startIndex += defaultScheduleList.Count + 1;
                        }
                        while (startIndex < mergedScheduleList.Count);
                    }
                    else
                    {
                        var defaultStart = curScheduleList.Last().End;
                        var defaultEnd = end;
                        var defaultScheduleList = GetDefaultScheduleListBetween(defaultStart, defaultEnd);
                        mergedScheduleList.InsertRange(mergedScheduleList.Count, defaultScheduleList);
                    }
                }
                else {
                    if (nextScheduleList.Count > 0)
                    {
                        var defaultStart = start;
                        var defaultEnd = nextScheduleList.First().Start;
                        var defaultScheduleList = GetDefaultScheduleListBetween(defaultStart, defaultEnd);
                        if (defaultScheduleList.Count > 0) defaultScheduleList.Last().BringForwardEndTime(defaultEnd);
                        mergedScheduleList.InsertRange(0, defaultScheduleList);

                        var startIndex = defaultScheduleList.Count;
                        do
                        {
                            defaultStart = mergedScheduleList[startIndex].End;
                            defaultEnd = startIndex + 1 >= mergedScheduleList.Count ? end : mergedScheduleList[startIndex + 1].Start;
                            defaultScheduleList = GetDefaultScheduleListBetween(defaultStart, defaultEnd);
                            if (defaultEnd < end && defaultScheduleList.Count > 0)
                            {
                                defaultScheduleList.Last().BringForwardEndTime(end);
                            }
                            mergedScheduleList.InsertRange(startIndex + 1, defaultScheduleList);
                            startIndex += defaultScheduleList.Count + 1;
                        }
                        while (startIndex < mergedScheduleList.Count);
                    }
                    else
                    {
                        var defaultStart = start;
                        var defaultEnd = end;
                        var defaultScheduleList = GetDefaultScheduleListBetween(defaultStart, defaultEnd);
                        mergedScheduleList.InsertRange(0, defaultScheduleList);
                    }
                }
            }

            return mergedScheduleList;
        }


    }

    public class PlayScheduleManager
    {

        private static PlayScheduleManager? _instance;
        public static PlayScheduleManager Instance { get => _instance ??= new PlayScheduleManager(); }

        public List<LayerScheduler> Schedulers { get; set; } = [];

        public PlayScheduleManager() { }

        public void Play(List<DisplayLayer> layers)
        {
            DateTime start = DateTime.Now + TimeSpan.FromSeconds(5);
            TimeSpan interval = TimeSpan.FromMinutes(2);
            foreach (var layer in layers)
            {
                var layerScheduler = new LayerScheduler(layer, start, interval);
                Schedulers.Add(layerScheduler);
            }
        }

        public void Stop()
        {
            foreach (var scheduler in Schedulers)
            {
                scheduler.Dispose();
            }
            Schedulers.Clear();
        }

    }

}
