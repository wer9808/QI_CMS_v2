using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QI_CMS_v2.Utils
{
    public class DataStructure
    {
        public class ConsumerFirstQueue<T>
        {
            // 내부 데이터 저장소
            private readonly Queue<T> _queue;

            // 동기화를 위한 객체
            private readonly object _lock = new object();

            // 소비자 대기 신호
            private readonly SemaphoreSlim _itemAvailable;

            // 대기 중인 소비자 수
            private int _waitingConsumers;

            // 생산자 대기 신호 (큐 크기 제한 시 사용)
            private readonly SemaphoreSlim _spaceAvailable;

            // 최대 큐 크기 (-1은 무제한)
            private readonly int _maxCapacity;

            /// <summary>
            /// 소비자 우선 큐 생성자
            /// </summary>
            /// <param name="maxCapacity">최대 큐 크기 (기본값: 무제한)</param>
            public ConsumerFirstQueue(int maxCapacity = -1)
            {
                _queue = new Queue<T>();
                _maxCapacity = maxCapacity;
                _waitingConsumers = 0;
                _itemAvailable = new SemaphoreSlim(0);

                // 최대 크기가 지정된 경우 공간 가용성 세마포어 초기화
                _spaceAvailable = maxCapacity > 0
                    ? new SemaphoreSlim(maxCapacity)
                    : null;
            }

            /// <summary>
            /// 현재 큐에 저장된 항목 수를 반환합니다.
            /// </summary>
            public int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _queue.Count;
                    }
                }
            }

            /// <summary>
            /// 큐가 비어있는지 확인합니다.
            /// </summary>
            public bool IsEmpty
            {
                get
                {
                    lock (_lock)
                    {
                        return _queue.Count == 0;
                    }
                }
            }

            /// <summary>
            /// 큐가 최대 용량에 도달했는지 확인합니다.
            /// </summary>
            public bool IsFull
            {
                get
                {
                    if (_maxCapacity <= 0) return false;

                    lock (_lock)
                    {
                        return _queue.Count >= _maxCapacity;
                    }
                }
            }
            /// <summary>
            /// 큐에 항목을 추가합니다 (생산자 메서드).
            /// </summary>
            /// <param name="item">추가할 항목</param>
            /// <param name="timeout">제한 시간 (밀리초)</param>
            /// <returns>항목 추가 성공 여부</returns>
            public bool Enqueue(T item, int timeout = Timeout.Infinite)
            {
                // 최대 크기 제한이 있을 때 공간 확보
                if (_maxCapacity > 0)
                {
                    if (!_spaceAvailable.Wait(timeout))
                    {
                        return false; // 타임아웃 - 공간 확보 실패
                    }
                }

                bool shouldSignal = false;

                lock (_lock)
                {
                    // 큐에 항목 추가
                    _queue.Enqueue(item);

                    // 아이템을 추가한 후 대기 중인 소비자가 있으면 신호를 보내야 함
                    shouldSignal = _waitingConsumers > 0;

                    Console.WriteLine($"[생산자] 항목 추가: {item}, 큐 크기: {_queue.Count}, 대기 소비자: {_waitingConsumers}, 신호: {shouldSignal}");
                }

                // 중요: 락 외부에서 신호를 보내 데드락 방지
                if (shouldSignal)
                {
                    _itemAvailable.Release();
                    Console.WriteLine($"[생산자] 항목 {item}에 대한 신호 전송됨");
                }

                return true;
            }

            /// <summary>
            /// 큐에서 항목을 동기적으로 가져옵니다.
            /// </summary>
            /// <param name="timeout">제한 시간 (밀리초)</param>
            /// <returns>가져온 항목과 성공 여부</returns>
            public (T item, bool success) Dequeue(int timeout = Timeout.Infinite)
            {
                T item = default;
                bool success = false;

                lock (_lock)
                {
                    // 먼저 큐에 항목이 있는지 확인
                    if (_queue.Count > 0)
                    {
                        // 항목이 있으면 즉시 가져오기
                        item = _queue.Dequeue();

                        // 최대 크기 제한이 있을 때 공간 추가
                        if (_maxCapacity > 0)
                        {
                            _spaceAvailable.Release();
                        }

                        Console.WriteLine($"[소비자] 항목 즉시 가져옴: {item}, 남은 큐 크기: {_queue.Count}");
                        return (item, true);
                    }

                    // 큐가 비어있으면 대기 소비자로 등록
                    Interlocked.Increment(ref _waitingConsumers);
                    Console.WriteLine($"[소비자] 대기 시작, 대기 소비자: {_waitingConsumers}");
                }

                try
                {
                    // 락 외부에서 신호 대기
                    Console.WriteLine("[소비자] 신호 대기 중...");
                    success = _itemAvailable.Wait(timeout);

                    if (!success)
                    {
                        Console.WriteLine("[소비자] 대기 시간 초과");
                        return (default, false);
                    }

                    Console.WriteLine("[소비자] 신호 받음, 항목 가져오기 시도");

                    // 신호를 받았으면 락을 획득하고 항목 가져오기
                    lock (_lock)
                    {
                        if (_queue.Count > 0)
                        {
                            item = _queue.Dequeue();
                            success = true;

                            // 최대 크기 제한이 있을 때 공간 추가
                            if (_maxCapacity > 0)
                            {
                                _spaceAvailable.Release();
                            }

                            Console.WriteLine($"[소비자] 항목 가져옴: {item}, 남은 큐 크기: {_queue.Count}");
                        }
                        else
                        {
                            // 비정상적인 상황 - 신호를 받았는데 큐가 비어있음
                            Console.WriteLine("[소비자] 신호를 받았으나 큐가 비어있음! 가능한 경쟁 상태 발생");
                            success = false;
                        }
                    }
                }
                finally
                {
                    // 대기 소비자 수 감소
                    Interlocked.Decrement(ref _waitingConsumers);
                    Console.WriteLine($"[소비자] 대기 상태 종료, 남은 대기 소비자: {_waitingConsumers}");
                }

                return (item, success);
            }

            /// <summary>
            /// 큐에서 항목을 비동기적으로 가져옵니다.
            /// </summary>
            /// <param name="timeout">제한 시간 (밀리초)</param>
            /// <param name="cancellationToken">취소 토큰</param>
            /// <returns>가져온 항목과 성공 여부</returns>
            public async Task<(T item, bool success)> DequeueAsync(
                int timeout = Timeout.Infinite,
                CancellationToken cancellationToken = default)
            {
                T item = default;
                bool success = false;

                lock (_lock)
                {
                    // 즉시 항목을 가져올 수 있는지 확인
                    if (_queue.Count > 0)
                    {
                        item = _queue.Dequeue();

                        // 최대 크기 제한이 있을 때 공간 추가
                        if (_maxCapacity > 0)
                        {
                            _spaceAvailable.Release();
                        }

                        Console.WriteLine($"[소비자] 항목 즉시 가져옴: {item}, 남은 큐 크기: {_queue.Count}");
                        return (item, true);
                    }

                    // 대기 중인 소비자 수 증가
                    Interlocked.Increment(ref _waitingConsumers);
                    Console.WriteLine($"[소비자] 비동기 대기 시작, 대기 소비자: {_waitingConsumers}");
                }

                try
                {
                    // 큐에 항목이 들어올 때까지 비동기 대기
                    Console.WriteLine("[소비자] 비동기 신호 대기 중...");
                    success = await _itemAvailable.WaitAsync(timeout, cancellationToken);

                    if (!success)
                    {
                        Console.WriteLine("[소비자] 비동기 대기 시간 초과");
                        return (default, false);
                    }

                    Console.WriteLine("[소비자] 비동기 신호 받음, 항목 가져오기 시도");

                    lock (_lock)
                    {
                        if (_queue.Count > 0)
                        {
                            item = _queue.Dequeue();
                            success = true;

                            // 최대 크기 제한이 있을 때 공간 추가
                            if (_maxCapacity > 0)
                            {
                                _spaceAvailable.Release();
                            }

                            Console.WriteLine($"[소비자] 항목 비동기 가져옴: {item}, 남은 큐 크기: {_queue.Count}");
                        }
                        else
                        {
                            // 비정상적인 상황 - 신호를 받았는데 큐가 비어있음
                            Console.WriteLine("[소비자] 비동기 신호를 받았으나 큐가 비어있음! 가능한 경쟁 상태 발생");
                            success = false;
                        }
                    }
                }
                finally
                {
                    // 대기 중인 소비자 수 감소
                    Interlocked.Decrement(ref _waitingConsumers);
                    Console.WriteLine($"[소비자] 비동기 대기 상태 종료, 남은 대기 소비자: {_waitingConsumers}");
                }

                return (item, success);
            }


            /// <summary>
            /// 큐를 초기화하고 모든 대기 중인 작업을 취소합니다.
            /// </summary>
            public void Clear()
            {
                lock (_lock)
                {
                    _queue.Clear();
                }

                // 필요시 추가 정리 작업
            }
        }
    }
}
