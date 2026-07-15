using System;
using System.Collections.Generic;
using System.Text;

namespace Gomoku
{
    internal class GomokuGame
    {
        /* 오목 클래스
         * 
         * 보드를 이차원 배열로 관리
         * 이어지는 방향을 이차원 배열로 관리 [뒤집기 여부, 4가지 방향]
         * 
         * 돌 위치 및 색깔 : Stone > record로 관리
         * 방향 : Direction > enum
         * 
         * 스트레이트 체크 메서드 : int CheckStraight(Stone playedStone); > 반환값 : 몇개가 붙어있는지
         *      돌면서 발견한 방향으로 보기, 찾았으면 그 방향으로 한 칸 찾을 때 마다 1부터 1씩 증가, 다른 색 돌이나 빈 공간 만나면 반대편
         *      반대편 보고 증가시키기
         *      4방향 다 확인하고 가장 큰 값 반환
         * 
         * 띈 돌 체크 메서드 : int CheckBroken(Stone PlayedStone); > 반환값 : 중간에 최대 1칸 띄어져 있을 때 몇개가 붙어있는지
         *      스트레이트 체크와 비슷하지만 2칸 탐색, 2칸 건너서 한 번 발견한 뒤로는 1칸씩만 탐색 (반대편 포함)
         *      8방향으로 다 확인하고 가장 큰 값 반환
         * 
         * 띈 돌 체크 메서드 2 : int CheckBrokenOpen(Stone PlayedStone, Direction direction) > 반환값 : 해당 방향으로 몇 개 있는지
         *      2칸 탐색하고 띄워져 있는 거 찾으면 그때부터 1칸 탐색 (반대편 포함)
         *      마지막으로 못찾으면 그 방향 2칸 전부 비워져있는지 확인, 안 비워져있으면 -1 반환
         *      그 후 반환
         */

        enum Direction
        {
            LeftUp,
            Up,
            RightUp,
            Right,
            RightDown,
            Down,
            LeftDown,
            Left
        }

        enum StoneType
        {
            None,
            Black,
            White
        }

        record struct Stone(StoneType type, short x, short y);

        private const StoneType N = StoneType.None;
        private const StoneType B = StoneType.Black;
        private const StoneType W = StoneType.White;

        private StoneType[,] board = new StoneType[15, 15];
        private short[,][] direction = new short[2, 4][] {
            { 
                new short[2] { -1, 1 }, 
                new short[2] { 0, 1 }, 
                new short[2] { 1, 1 }, 
                new short[2] { 1, 0 } 
            }, 
            { 
                new short[2] { 1, -1 }, 
                new short[2] { 0, -1 }, 
                new short[2] { -1, -1 }, 
                new short[2] { -1, 0 } 
            } 
        };

        public bool isBlackTurn { get; private set; } = true;
        public bool isEnded { get; private set; } = false;


        public void PlayStone(short x, short y)
        {
            if (board[y, x] != StoneType.None)
            {
                PrintMessage("이미 놓여진 돌이 있습니다!", -2);
                return;
            }

            if (isBlackTurn)
            {
                short[,] directionOpenStoneTotal = new short[2, 4];
                short[,] directionTwoRangeOpenStoneTotal = new short[2, 4];
                short[,] directionStoneTotal = new short[2, 4];

                List<Stone>[,] checkedOpenStones = new List<Stone>[2, 4]; // 후에 할당
                List<Stone>[,] checkedTwoRangeOpenStones = new List<Stone>[2, 4];

                bool[,] isOpenJumpedBeforeTurningArray = new bool[2, 4];
                bool[,] isTwoRangeOpenJumpedBeforeTurningArray = new bool[2, 4];


                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        directionOpenStoneTotal[i, j] = CheckBrokenOpen(new Stone(StoneType.Black, x, y), (short)(i * 4 + j), out checkedOpenStones[i, j], out isOpenJumpedBeforeTurningArray[i, j]);
                        directionTwoRangeOpenStoneTotal[i, j] = CheckBrokenOpen(new Stone(StoneType.Black, x, y), (short)(i * 4 + j), out checkedTwoRangeOpenStones[i, j], out isTwoRangeOpenJumpedBeforeTurningArray[i, j], 2);
                        directionStoneTotal[i, j] = CheckBroken(new Stone(StoneType.Black, x, y), (short)(i * 4 + j));
                    }
                }

                int found3 = -1;
                int found4 = -1;

                bool is3JumpedBeforeTurning = false;
                bool is4JumpedBeforeTurning = false;

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        short foundOpenStone = directionOpenStoneTotal[i, j];
                        short foundTwoRangeOpenStone = directionTwoRangeOpenStoneTotal[i, j];
                        short foundStone = directionStoneTotal[i, j];

                        if (foundTwoRangeOpenStone == 3)
                        {
                            if (found3 == -1)
                            {
                                found3 = j;
                                is3JumpedBeforeTurning = isTwoRangeOpenJumpedBeforeTurningArray[i, j];

                                if (FoundSecond(checkedTwoRangeOpenStones[i, j], 3, "33 금수입니다!", 2))
                                {
                                    return;
                                }
                            }
                            else if (found3 != j || (found3 == j && isTwoRangeOpenJumpedBeforeTurningArray[i, j] && is3JumpedBeforeTurning))
                            {
                                PrintMessage("33 금수입니다!", -2);
                                return;
                            }
                        }
                        else if (foundOpenStone == 4) 
                        {
                            if (found4 == -1)
                            {
                                found4 = j;
                                is4JumpedBeforeTurning = isOpenJumpedBeforeTurningArray[i, j];

                                if (FoundSecond(checkedOpenStones[i, j], 4, "44 금수입니다!", 1))
                                {
                                    return;
                                }
                            }
                            else if (found4 != j || (found4 == j && isOpenJumpedBeforeTurningArray[i, j] && is4JumpedBeforeTurning))
                            {
                                PrintMessage("44 금수입니다!", -2);
                                return;
                            }
                        }
                        else if (foundStone >= 6)
                        {
                            PrintMessage("장목 금수입니다!", -2);
                            return;
                        }
                    }
                }
            }

            if (isBlackTurn) board[y, x] = StoneType.Black;
            else board[y, x] = StoneType.White;

            if (CheckStraight(new Stone(board[y,x], x, y)) >= 5)
            {
                isEnded = true;

                if (isBlackTurn) PrintMessage("흑 승리!");
                else PrintMessage("백 승리!");

                return;
            }

            if (isBlackTurn) isBlackTurn = false;
            else isBlackTurn = true;

            bool FoundSecond(List<Stone> stonesList, int criteria, string message, short openRange = 1)
            {
                short[,,] secondLastStoneDirectionCheck = new short[stonesList.Count, 2, 4];

                for (int a = 0; a < stonesList.Count; a++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            secondLastStoneDirectionCheck[a, k, l] = CheckBrokenOpen(stonesList[a], (short)(k * 4 + l), out bool isJumped, openRange);
                        }
                    }

                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            if (secondLastStoneDirectionCheck[a, k, l] == criteria)
                            {
                                PrintMessage(message, -2);
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        public void DrawScreen()
        {
            Console.SetCursorPosition(0, 0);

            Console.Write("  ");
            for (int i = 0; i < 15; i++) Console.Write($"{i + 1, 2} ");
            Console.WriteLine();

            for (int i = 0; i < 15; i++)
            {
                Console.Write($"{i + 1, 2} ");

                for (int j = 0; j < 15; j++)
                {
                    switch (board[i, j])
                    {
                        case StoneType.None:
                            Console.Write("-  ");
                            break;
                        case StoneType.Black:
                            Console.Write("○ ");
                            break;
                        case StoneType.White:
                            Console.Write("● ");
                            break;
                    }
                }

                Console.WriteLine();
            }
        }

        private short CheckBroken(Stone playedStone, short _direction)
        {
            /*
             * 띈 돌 체크 메서드 : int CheckBroken(Stone PlayedStone, short direction); > 반환값 : 중간에 최대 1칸 띄어져 있을 때 그 방향을로 몇개가 붙어있는지
             *      스트레이트 체크와 비슷하지만 2칸 탐색, 2칸 건너서 한 번 발견한 뒤로는 1칸씩만 탐색 (반대편 포함)
             *      그 후 반환
             */

            short stoneTotal = 1;
            short checkX = playedStone.x;
            short checkY = playedStone.y;
            bool isAlreadyPassed = false;

            short[] dosArray = direction[_direction / 4, _direction % 4];
            short[] directionOfStone = new short[2] { dosArray[0], dosArray[1] }; // 값으로 수정 가능하게 전달

            for (int i = 0; i < 2; i++)
            {
                checkX = playedStone.x;
                checkY = playedStone.y;

                while (true)
                {
                    if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                    {
                        break;
                    }

                    if (board[checkY, checkX] == playedStone.type)
                    {
                        stoneTotal++;
                    }
                    else if (!isAlreadyPassed)
                    {
                        if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                        {
                            break;
                        }

                        if (board[checkY, checkX] == playedStone.type)
                        {
                            stoneTotal++;
                            isAlreadyPassed = true;
                        }
                    }
                }

                directionOfStone[0] = (short)-directionOfStone[0];
                directionOfStone[1] = (short)-directionOfStone[1];
            }

            return stoneTotal;

            bool TryChangeXY(short x, short y)
            {
                if ((0 <= x && x < 15) && (0 <= y && y < 15))
                {
                    checkX = x;
                    checkY = y;
                    return true;
                }

                return false;
            }
        }

        private short CheckBrokenOpen(Stone playedStone, short _direction, out bool isJumpedBeforeTurning, short openRange = 1)
        {
        /*
         * 띈 돌 체크 메서드 2 : int CheckBrokenOpen(Stone PlayedStone, short direction) > 반환값 : 해당 방향으로 몇 개 있는지
         * 2칸 탐색하고 띄워져 있는 거 찾으면 그때부터 1칸 탐색(반대편 포함)
         * 마지막으로 못찾으면 그 방향 2칸 전부 비워져있는지 확인, 안 비워져있으면 - 1 반환
         * 그 후 반환
         */
            short stoneTotal = 1;
            short checkX = playedStone.x;
            short checkY = playedStone.y;
            bool isOneBlocked = false;
            bool isJumped = false;

            short[] dosArray = direction[_direction / 4, _direction % 4];
            short[] directionOfStone = new short[2] { dosArray[0], dosArray[1] }; // 값으로 수정 가능하게 전달

            isJumpedBeforeTurning = false;
            
            for (int i = 0; i < 2; i++)
            {
                checkX = playedStone.x;
                checkY = playedStone.y;

                while (true)
                {
                    if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                    {
                        if (!isOneBlocked) isOneBlocked = true;
                        else
                        {
                            return -1;
                        }

                        break;
                    }

                    if (board[checkY, checkX] == playedStone.type)
                    {
                        stoneTotal++;
                    }
                    else if (!isJumped)
                    {
                        if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                        {
                            if (!isOneBlocked) isOneBlocked = true;
                            else
                            {
                                return -1;
                            }

                            break;
                        }

                        if (board[checkY, checkX] == playedStone.type)
                        {
                            stoneTotal++;
                            isJumped = true;
                        }
                        else
                        {
                            if (!TryChangeXY((short)(checkX - directionOfStone[0] * 2), (short)(checkY - directionOfStone[1] * 2)))
                            {
                                if (!isOneBlocked) isOneBlocked = true;
                                else
                                {
                                    return -1;
                                }

                                break;
                            }

                            for (int j = 0; j < openRange; j++)
                            {
                                if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                                {
                                    if (!isOneBlocked) isOneBlocked = true;
                                    else
                                    {
                                        return -1;
                                    }

                                    break;
                                }

                                if (playedStone.type == StoneType.Black)
                                {
                                    if (board[checkY, checkX] == StoneType.White)
                                    {
                                        if (!isOneBlocked) isOneBlocked = true;
                                        else
                                        {
                                            return -1;
                                        }

                                        break;
                                    }
                                }
                                else
                                {
                                    if (board[checkY, checkX] == StoneType.Black)
                                    {
                                        if (!isOneBlocked) isOneBlocked = true;
                                        else
                                        {
                                            return -1;
                                        }
                                        break;
                                    }

                                }
                            }

                            break;
                        }
                    }
                    else
                    {
                        if (!TryChangeXY((short)(checkX - directionOfStone[0]), (short)(checkY - directionOfStone[1])))
                        {
                            if (!isOneBlocked) isOneBlocked = true;
                            else
                            {
                                return -1;
                            }

                            break;
                        }

                        for (int j = 0; j < openRange; j++)
                        {
                            if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                            {
                                if (!isOneBlocked) isOneBlocked = true;
                                else
                                {
                                    return -1;
                                }

                                break;
                            }

                            if (playedStone.type == StoneType.Black)
                            {
                                if (board[checkY, checkX] == StoneType.White)
                                {
                                    if (!isOneBlocked) isOneBlocked = true;
                                    else
                                    {
                                        return -1;
                                    }

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }

                directionOfStone[0] = (short)-directionOfStone[0];
                directionOfStone[1] = (short)-directionOfStone[1];

                if (i == 0 && isJumped)
                {
                    isJumpedBeforeTurning = true;
                }
            }

            return stoneTotal;

            bool TryChangeXY(short x, short y)
            {
                if ((0 <= x && x < 15) && (0 <= y && y < 15))
                {
                    checkX = x;
                    checkY = y;
                    return true;
                }

                return false;
            }
        }

        private short CheckBrokenOpen(Stone playedStone, short _direction, out List<Stone> CheckedStones, out bool isJumpedBeforeTurning, short openRange = 1)
        {
            /*
             * 띈 돌 체크 메서드 2 : int CheckBrokenOpen(Stone PlayedStone, short direction) > 반환값 : 해당 방향으로 몇 개 있는지
             * 2칸 탐색하고 띄워져 있는 거 찾으면 그때부터 1칸 탐색(반대편 포함)
             * 마지막으로 못찾으면 그 방향 2칸 전부 비워져있는지 확인, 안 비워져있으면 - 1 반환
             * 그 후 반환
             */
            short stoneTotal = 1;
            short checkX = playedStone.x;
            short checkY = playedStone.y;
            bool isOneBlocked = false;
            bool isJumped = false;

            CheckedStones = new(10);

            short[] dosArray = direction[_direction / 4, _direction % 4];
            short[] directionOfStone = new short[2] { dosArray[0], dosArray[1] }; // 값으로 수정 가능하게 전달

            isJumpedBeforeTurning = false;

            for (int i = 0; i < 2; i++)
            {
                checkX = playedStone.x;
                checkY = playedStone.y;

                int debug = 0;
                while (true)
                {
                    debug++;

                    if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1]))) // 이동
                    {
                        if (!isOneBlocked) isOneBlocked = true;
                        else
                        {
                            return -1;
                        }

                        break;
                    }

                    if (board[checkY, checkX] == playedStone.type) // 같은 색이면
                    {
                        stoneTotal++;

                        CheckedStones.Add(new Stone(StoneType.Black, checkX, checkY));
                    }
                    else if (!isJumped) // 빈 공간이지만 건너뛴 적 없으면
                    {
                        if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1]))) // 한 번 더 이동
                        {
                            if (!isOneBlocked) isOneBlocked = true;
                            else
                            {
                                return -1;
                            }

                            break;
                        }

                        if (board[checkY, checkX] == playedStone.type) // 같은 색이면
                        {
                            stoneTotal++;
                            isJumped = true;

                            CheckedStones.Add(new Stone(StoneType.Black, checkX, checkY));
                        }
                        else // 건너뛰어 확인했는데도 아니면
                        {
                            if (!TryChangeXY((short)(checkX - directionOfStone[0] * 2), (short)(checkY - directionOfStone[1] * 2))) // 뒤로 가기
                            { // 판의 끝이라면
                                if (!isOneBlocked) isOneBlocked = true;
                                else
                                {
                                    return -1;
                                }

                                break;
                            }

                            for (int j = 0; j < openRange; j++)
                            {
                                if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1]))) // 앞으로 가기
                                {
                                    if (!isOneBlocked) isOneBlocked = true;
                                    else
                                    {
                                        return -1;
                                    }

                                    break;
                                }

                                if (playedStone.type == StoneType.Black)
                                {
                                    if (board[checkY, checkX] == StoneType.White)
                                    {
                                        if (!isOneBlocked) isOneBlocked = true;
                                        else
                                        {
                                            return -1;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    }
                    else
                    {
                        if (!TryChangeXY((short)(checkX - directionOfStone[0]), (short)(checkY - directionOfStone[1])))
                        {
                            if (!isOneBlocked) isOneBlocked = true;
                            else
                            {
                                return -1;
                            }

                            break;
                        }

                        for (int j = 0; j < openRange; j++)
                        {
                            if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                            {
                                if (!isOneBlocked) isOneBlocked = true;
                                else
                                {
                                    return -1;
                                }

                                break;
                            }

                            if (playedStone.type == StoneType.Black)
                            {
                                if (board[checkY, checkX] == StoneType.White)
                                {
                                    if (!isOneBlocked) isOneBlocked = true;
                                    else
                                    {
                                        return -1;
                                    }

                                    break;
                                }
                            }
                            else
                            {
                                if (board[checkY, checkX] == StoneType.Black)
                                {
                                    if (!isOneBlocked) isOneBlocked = true;
                                    else
                                    {
                                        return -1;
                                    }
                                    break;
                                }

                            }
                        }

                        break;
                    }
                }

                directionOfStone[0] = (short)-directionOfStone[0];
                directionOfStone[1] = (short)-directionOfStone[1];

                if (i == 0 && isJumped)
                {
                    isJumpedBeforeTurning = true;
                }
            }

            return stoneTotal;

            bool TryChangeXY(short x, short y)
            {
                if ((0 <= x && x < 15) && (0 <= y && y < 15))
                {
                    checkX = x;
                    checkY = y;
                    return true;
                }

                return false;
            }
        }


        private int CheckStraight(Stone playedStone)
        {
            List<(int, int)> lastDirection = new(3);
            short checkX = playedStone.x;
            short checkY = playedStone.y;
            int stoneMax = 1;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    checkX = playedStone.x;
                    checkY = playedStone.y;

                    if (!TryChangeXY((short)(checkX + direction[i, j][0]), (short)(checkY + direction[i, j][1])))
                    {
                        continue;
                    }
                    
                    if (board[checkY, checkX] == playedStone.type)
                    {
                        lastDirection.Add((i, j));
                    }
                }
            }

            for (int i = 0; i < lastDirection.Count; i++)
            {
                short[] directionOfStone = direction[lastDirection[i].Item1, lastDirection[i].Item2];
                short stoneOfThisDirection = 1;

                for (int j = 0; j < 2; j++)
                {
                    checkX = playedStone.x;
                    checkY = playedStone.y;

                    while (true)
                    {
                        if (!TryChangeXY((short)(checkX + directionOfStone[0]), (short)(checkY + directionOfStone[1])))
                        {
                            break;
                        }

                        if (board[checkY, checkX] == playedStone.type)
                        {
                            stoneOfThisDirection++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    directionOfStone[0] = (short)(-directionOfStone[0]);
                    directionOfStone[1] = (short)(-directionOfStone[1]);
                }

                if (stoneMax < stoneOfThisDirection)
                {
                    stoneMax = stoneOfThisDirection;
                }
            }

            return stoneMax;

            bool TryChangeXY(short x, short y)
            {
                if ((0 <= x && x < 15) && (0 <= y && y < 15))
                {
                    checkX = x;
                    checkY = y;
                    return true;
                }

                return false;
            }
        }

        public static void PrintMessage(string message, int line = 0)
        {
            Console.SetCursorPosition(0, 19 + line);
            Console.WriteLine(new string(' ', Console.BufferWidth));

            Console.SetCursorPosition(0, 19 + line);
            Console.WriteLine(message);
        }

        public static void PrintMessageWithoutLineBreak(string message, int line = 0)
        {
            Console.SetCursorPosition(0, 19 + line);
            Console.WriteLine(new string(' ', Console.BufferWidth));

            Console.SetCursorPosition(0, 19 + line);
            Console.Write(message);
        }

        public void Debug33()
        {
            board = new StoneType[15, 15]
            {
                { N, N, N, N, N, N, N, N, W, N, N, N, N, N, W },
                { N, B, N, N, N, N, N, N, W, N, B, N, B, N, W },
                { N, N, N, B, B, N, N, N, W, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, W, N, N, B, N, N, W },
                { N, B, N, N, N, N, N, N, N, N, N, B, N, N, N },
                { N, N, N, N, N, B, B, N, N, B, N, N, N, N, W },
                { N, N, N, N, N, N, N, B, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, B, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, W, N, N, N, N, W },
                { N, N, N, N, B, N, N, N, N, N, N, N, N, N, N },
                { N, N, N, B, N, B, N, N, N, B, N, N, N, N, W },
                { N, N, N, B, N, N, N, N, B, N, B, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, B, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, W, N, W, W, W, N }
            };
        }

        public void Debug44andLong()
        {
            board = new StoneType[15, 15]
            {
                { N, N, B, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, B, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, B, B, N, N, B, N, B, B, N, N, N, N, W },
                { N, N, B, N, N, N, N, N, N, N, N, N, N, N, N },
                { N, N, B, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, W, N, B, N, N, N, N, W },
                { N, N, N, N, N, N, N, B, N, B, B, N, N, N, W },
                { N, N, N, N, N, N, N, B, N, N, N, N, N, N, W },
                { N, N, N, N, B, N, N, B, N, B, N, N, N, N, N },
                { N, N, N, W, B, B, B, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, B, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, N, N, N, N, N, N, N, W },
                { N, N, N, N, N, N, N, W, W, N, W, W, W, W, N }
            }
            ;
        }
    }
}
