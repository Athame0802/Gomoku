namespace Gomoku
{
    internal class Program
    {
        static GomokuGame gomoku = default;

        static void Main(string[] args)
        {
            gomoku = new();
            gomoku.DrawScreen();

            while (!gomoku.isEnded)
            {
                bool isSucceed = TryInputCoord(out (short, short) inputCoord);

                if (isSucceed)
                {
                    if ((0 <= inputCoord.Item1 && inputCoord.Item1 < 15) && (0 <= inputCoord.Item2 && inputCoord.Item2 < 15))
                    {
                        GomokuGame.PrintMessage("", -2);
                        gomoku.PlayStone(inputCoord.Item1, inputCoord.Item2);
                    }

                    else
                    {
                        GomokuGame.PrintMessage("잘못된 입력입니다!", -2);
                    }
                }
                else
                {
                    GomokuGame.PrintMessage("잘못된 입력입니다!", -2);
                }

                gomoku.DrawScreen();
            }

            Console.SetCursorPosition(0, 20);
        }

        static bool TryInputCoord(out (short, short) inputCoord)
        {
            if (gomoku.isBlackTurn) GomokuGame.PrintMessageWithoutLineBreak("흑의 착수 지점 입력 ( ,로 구분 / x, y 순서 ) > ");
            else GomokuGame.PrintMessageWithoutLineBreak("백의 착수 지점 입력 ( ,로 구분 / x, y 순서 ) > ");
            string? input = Console.ReadLine();

            if (input == null)
            {
                inputCoord = default;
                return false;
            }
            else if (input == "debug33")
            {
                gomoku.Debug33();
            }
            else if (input == "debug44andLong")
            {
                gomoku.Debug44andLong();
            }

            string[] inputArray = input.Split(',');

            if (inputArray.Length != 2)
            {
                inputCoord = default;
                return false;
            }

            bool isFirstInputAlright = short.TryParse(inputArray[0], out short firstInput);
            bool isSecondInputAlright =short.TryParse(inputArray[1], out short secondInput);

            bool isAlright = isFirstInputAlright && isSecondInputAlright;

            if (!isAlright)
            {
                inputCoord = default;
                return false;
            }

            inputCoord = ((short)(firstInput - 1), (short)(secondInput - 1));
            return true;
        }
    }
}
