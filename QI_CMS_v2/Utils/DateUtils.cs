using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QI_CMS_v2.Utils
{
    internal class DateUtils
    {
        public static DayOfWeek[] DAY_OF_WEEKS = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));


        public static string GetKoreanDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "일",
                DayOfWeek.Monday => "월",
                DayOfWeek.Tuesday => "화",
                DayOfWeek.Wednesday => "수",
                DayOfWeek.Thursday => "목",
                DayOfWeek.Friday => "금",
                DayOfWeek.Saturday => "토",
                _ => throw new NotImplementedException()
            };
        }


        public static DayOfWeek GetDayOfWeekFromKorean(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "일" => DayOfWeek.Sunday,
                "월" => DayOfWeek.Monday,
                "화" => DayOfWeek.Tuesday,
                "수" => DayOfWeek.Wednesday,
                "목" => DayOfWeek.Thursday,
                "금" => DayOfWeek.Friday,
                "토" => DayOfWeek.Saturday,
                _ => throw new NotImplementedException()
            };
        }

        public static int CountSpecificDayOccurrences(DateTime start, DateTime end, int day)
        {
            int count = 0;
            DateTime current = start;

            while (current <= end)
            {
                // 현재 월에 해당 day가 존재하는지 확인
                if (DateTime.DaysInMonth(current.Year, current.Month) >= day)
                {
                    DateTime targetDate = new DateTime(current.Year, current.Month, day);

                    // targetDate가 start와 end 사이에 있는지 확인
                    if (targetDate >= start && targetDate <= end)
                    {
                        count++;
                    }
                }

                // 다음 달의 첫 날로 이동
                current = new DateTime(current.Year, current.Month, 1).AddMonths(1);
            }

            return count;
        }

        public class DateTimeSpanKey : IComparable<DateTimeSpanKey>, IEquatable<DateTimeSpanKey>
        {
            public DateTime DateTime { get; }
            public TimeSpan TimeSpan { get; }

            public DateTimeSpanKey(DateTime dateTime, TimeSpan timeSpan)
            {
                DateTime = dateTime;
                TimeSpan = timeSpan;
            }

            // IComparable 구현 - 정렬 순서를 정의
            public int CompareTo(DateTimeSpanKey other)
            {
                if (other == null)
                    return 1;

                // 먼저 DateTime으로 비교
                int dateTimeComparison = DateTime.CompareTo(other.DateTime);
                if (dateTimeComparison != 0)
                    return dateTimeComparison;

                // DateTime이 같으면 TimeSpan으로 비교
                // TimeSpan은 큰 값이 더 앞에 오도록 내림차순 정렬
                return other.TimeSpan.CompareTo(TimeSpan);
            }

            // IEquatable 구현
            public bool Equals(DateTimeSpanKey other)
            {
                if (other == null)
                    return false;

                return DateTime.Equals(other.DateTime) && TimeSpan.Equals(other.TimeSpan);
            }

            // Object.Equals 재정의
            public override bool Equals(object obj)
            {
                return obj is DateTimeSpanKey key && Equals(key);
            }

            // GetHashCode 재정의
            public override int GetHashCode()
            {
                return HashCode.Combine(DateTime, TimeSpan);
            }

            // ToString 재정의
            public override string ToString()
            {
                return $"{DateTime:yyyy-MM-dd HH:mm:ss} / {TimeSpan}";
            }
        }

        /// <summary>
        /// DateTime과 TimeSpan을 복합 키로 사용하는 이진 트리
        /// </summary>
        public class DateTimeSpanTree<T>
        {
            // SortedDictionary는 내부적으로 이진 트리 구조를 사용
            private SortedDictionary<DateTimeSpanKey, T> tree;

            public DateTimeSpanTree()
            {
                tree = new SortedDictionary<DateTimeSpanKey, T>();
            }

            // 요소 추가
            public void Add(DateTime dateTime, TimeSpan timeSpan, T value)
            {
                var key = new DateTimeSpanKey(dateTime, timeSpan);
                tree[key] = value;
            }

            // 요소 검색
            public bool TryGetValue(DateTime dateTime, TimeSpan timeSpan, out T value)
            {
                var key = new DateTimeSpanKey(dateTime, timeSpan);
                return tree.TryGetValue(key, out value);
            }

            // 요소 제거
            public bool Remove(DateTime dateTime, TimeSpan timeSpan)
            {
                var key = new DateTimeSpanKey(dateTime, timeSpan);
                return tree.Remove(key);
            }

            // 모든 키-값 쌍 열거
            public IEnumerable<KeyValuePair<DateTimeSpanKey, T>> GetAll()
            {
                return tree;
            }

            // 특정 날짜 이후의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTimeSpanKey, T>> GetAfter(DateTime date)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key.DateTime >= date)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 날짜 이전의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTimeSpanKey, T>> GetBefore(DateTime date)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key.DateTime <= date)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 날짜 범위 내의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTimeSpanKey, T>> GetRange(DateTime start, DateTime end)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key.DateTime >= start && pair.Key.DateTime <= end)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 DateTime 이전 값들 중 가장 DateTime이 큰 값을 가져오기
            public bool TryGetLatestBefore(DateTime date, out KeyValuePair<DateTimeSpanKey, T> result)
            {
                result = default;
                DateTimeSpanKey latestKey = null;

                foreach (var pair in tree)
                {
                    if (pair.Key.DateTime < date && (latestKey == null || pair.Key.DateTime > latestKey.DateTime))
                    {
                        latestKey = pair.Key;
                        result = pair;
                    }
                }

                return latestKey != null;
            }

            // 최적화된 버전 - 역방향 열거를 이용
            public bool TryGetLatestBeforeOptimized(DateTime date, out KeyValuePair<DateTimeSpanKey, T> result)
            {
                result = default;

                // 역순으로 순회
                if (tree.Count > 0)
                {
                    foreach (var pair in ((IEnumerable<KeyValuePair<DateTimeSpanKey, T>>)tree).Reverse())
                    {
                        if (pair.Key.DateTime < date)
                        {
                            result = pair;
                            return true;
                        }
                    }
                }

                return false;
            }

            // 트리의 크기 반환
            public int Count
            {
                get { return tree.Count; }
            }
        }



        public class DateTimeTree<T>
        {
            private SortedDictionary<DateTime, T> tree;

            public DateTimeTree()
            {
                tree = new SortedDictionary<DateTime, T>();
            }

            // 요소 추가
            public void Add(DateTime key, T value)
            {
                tree[key] = value;
            }

            // 요소 검색
            public bool TryGetValue(DateTime key, out T value)
            {
                return tree.TryGetValue(key, out value);
            }

            // 요소 제거
            public bool Remove(DateTime key)
            {
                return tree.Remove(key);
            }

            // 모든 키-값 쌍 열거
            public IEnumerable<KeyValuePair<DateTime, T>> GetAll()
            {
                return tree;
            }

            // 특정 날짜 이후의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTime, T>> GetAfter(DateTime date)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key >= date)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 날짜 이전의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTime, T>> GetBefore(DateTime date)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key <= date)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 날짜 범위 내의 모든 항목 가져오기
            public IEnumerable<KeyValuePair<DateTime, T>> GetRange(DateTime start, DateTime end)
            {
                foreach (var pair in tree)
                {
                    if (pair.Key >= start && pair.Key <= end)
                    {
                        yield return pair;
                    }
                }
            }

            // 특정 날짜 이전의 항목 중 가장 늦은 날짜를 키값으로 가진 항목 가져오기
            // 최적화된 버전 - 역방향 열거를 이용(C# 8.0 이상)
            public bool TryGetLatestBefore(DateTime date, out KeyValuePair<DateTime, T> result)
            {
                result = default;

                // SortedDictionary는 키 순으로 정렬되어 있으므로, 역순으로 순회하면 첫 번째로 발견되는
                // date보다 작은 항목이 가장 큰 값임
                if (tree.Count > 0)
                {
                    foreach (var pair in ((IEnumerable<KeyValuePair<DateTime, T>>)tree).Reverse())
                    {
                        if (pair.Key < date)
                        {
                            result = pair;
                            return true;
                        }
                    }
                }

                return false;
            }

            // 특정 날짜와 동일 또는 이전의 항목 중 가장 늦은 날짜를 키값으로 가진 항목 가져오기
            public bool TryGetLatestBeforeOrEqual(DateTime date, out KeyValuePair<DateTime, T> result)
            {
                result = default;

                // SortedDictionary는 키 순으로 정렬되어 있으므로, 역순으로 순회하면 첫 번째로 발견되는
                // date보다 작은 항목이 가장 큰 값임
                if (tree.Count > 0)
                {
                    foreach (var pair in ((IEnumerable<KeyValuePair<DateTime, T>>)tree).Reverse())
                    {
                        if (pair.Key <= date)
                        {
                            result = pair;
                            return true;
                        }
                    }
                }

                return false;
            }

            // 특정 날짜 이후의 항목 중 가장 이른 날짜를 키값으로 가진 항목 가져오기
            public bool TryGetEarliestAfter(DateTime date, out KeyValuePair<DateTime, T> result)
            {
                result = default;

                // SortedDictionary는 키 순으로 정렬되어 있으므로, 역순으로 순회하면 첫 번째로 발견되는
                // date보다 작은 항목이 가장 큰 값임
                if (tree.Count > 0)
                {
                    foreach (var pair in ((IEnumerable<KeyValuePair<DateTime, T>>)tree))
                    {
                        if (pair.Key > date)
                        {
                            result = pair;
                            return true;
                        }
                    }
                }

                return false;
            }

            // 특정 날짜와 동일 또는 이후의 항목 중 가장 이른 날짜를 키값으로 가진 항목 가져오기
            public bool TryGetEarliestAfterOrEqual(DateTime date, out KeyValuePair<DateTime, T> result)
            {
                result = default;

                // SortedDictionary는 키 순으로 정렬되어 있으므로, 역순으로 순회하면 첫 번째로 발견되는
                // date보다 작은 항목이 가장 큰 값임
                if (tree.Count > 0)
                {
                    foreach (var pair in ((IEnumerable<KeyValuePair<DateTime, T>>)tree))
                    {
                        if (pair.Key >= date)
                        {
                            result = pair;
                            return true;
                        }
                    }
                }

                return false;
            }

            // 트리의 크기 반환
            public int Count
            {
                get { return tree.Count; }
            }

        }

    }
}
