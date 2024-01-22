using System.Collections.Generic;

namespace MyApp
{
    internal static class Program
    {
        private const int WIDTH = 9;
        private const int HEIGHT = 8;

        private static readonly int MiddleX = Convert.ToInt32(Math.Floor(Convert.ToDouble(WIDTH / 2)));
        private static readonly int MiddleY = Convert.ToInt32(Math.Floor(Convert.ToDouble(HEIGHT / 2)));

        private const int EMPTY_ROOM_INDEX = 0;
        private const int ROOM_INDEX = 1;
        private const int BOSS_ROOM_INDEX = 2;
        private const int GOLDEN_ROOM_INDEX = 3;
        private const int SHOP_INDEX = 4;
        private const int SECRET_ROOM_INDEX = 5;

        private static readonly Random random = new();

        private static int[][] SetUpFloor()
        {
            int[][] floor = new int[HEIGHT][];

            for (int i = 0; i < floor.Length; i++)
            {
                floor[i] = new int[WIDTH];
            }

            for (int y = 0; y < floor.Length; y++)
            {
                for (int x = 0; x < floor[y].Length; x++)
                {
                    floor[y][x] = EMPTY_ROOM_INDEX;
                }
            }

            List<(int, int)> endrooms = FillFloor(floor);

            if (endrooms.Count == 0) return SetUpFloor();

            if (AddSpecialRooms(floor, endrooms) == false) return SetUpFloor();

            return floor;
        }

        private static List<(int, int)> FillFloor(int[][] floor)
        {
            Queue<(int, int)> checkingRoomQueue = new();

            int roomQuantity = random.Next(7, 15);
            int currentRoomQuantity = 1;

            var (MiddleX, MiddleY) = (Convert.ToInt32(Math.Floor(Convert.ToDouble(WIDTH / 2))), Convert.ToInt32(Math.Floor(Convert.ToDouble(HEIGHT / 2))));
            floor[MiddleY][MiddleX] = 1;
            checkingRoomQueue.Enqueue((MiddleX, MiddleY));

            List<(int, int)> endrooms = new();

            //Не останавливаем цикл при currentRoomQuantity == roomQuantity, потому-что нам нужно записать все endrooms для особых комнат
            while (checkingRoomQueue.Count > 0)
            {
                var (x, y) = checkingRoomQueue.Dequeue();

                bool createdAnyRoom = false;

                if (x > 0) createdAnyRoom = createdAnyRoom | TryToCreateRoom(x - 1, y);
                if (x < WIDTH - 1) createdAnyRoom = createdAnyRoom | TryToCreateRoom(x + 1, y);
                if (y > 0) createdAnyRoom = createdAnyRoom | TryToCreateRoom(x, y - 1);
                if (y < HEIGHT - 1) createdAnyRoom = createdAnyRoom | TryToCreateRoom(x, y + 1);

                if (createdAnyRoom == false) endrooms.Add((x, y));
            }

            if (currentRoomQuantity < roomQuantity) 
                return new List<(int, int)>(); //Restart

            return endrooms;

            bool TryToCreateRoom(int x, int y)
            {
                if (floor[y][x] > 0) return false;

                if (CountNeighbours(floor, x, y) > 1) return false;

                if (roomQuantity <= currentRoomQuantity) return false;

                if (random.Next(2) == 0) return false;

                currentRoomQuantity++;
                floor[y][x] = ROOM_INDEX;
                checkingRoomQueue.Enqueue((x, y));

                return true;
            }
        }

        private static bool AddSpecialRooms(int[][] floor, List<(int, int)> endrooms)
        {
            if (endrooms.Count < 3) return false;

            if (AddBossRoom() == false) return false;

            AddGoldenRoomAndMarket();

            if (AddSecretRoom() == false) return false;

            return true;

            bool AddBossRoom()
            {
                var (x, y) = ExtractEndroom(endrooms.Count - 1);

                if ((x == MiddleX - 1 && y == MiddleY) ||
                    (x == MiddleX && y == MiddleY - 1) ||
                    (x == MiddleX && y == MiddleY + 1) ||
                    (x == MiddleX + 1 && y == MiddleY)) return false;

                floor[y][x] = BOSS_ROOM_INDEX;

                return true;
            }

            void AddGoldenRoomAndMarket()
            {
                var (goldenX, goldenY) = ExtractEndroom(random.Next(endrooms.Count));
                floor[goldenY][goldenX] = GOLDEN_ROOM_INDEX;

                var (shopX, shopY) = ExtractEndroom(random.Next(endrooms.Count));
                floor[shopY][shopX] = SHOP_INDEX;
            }

            bool AddSecretRoom()
            {
                for (int i = 0; i < 900; i++)
                {
                    int x = random.Next(MiddleX);
                    int y = random.Next(MiddleY);

                    if (floor[y][x] > 0) continue;

                    if (x - 1 >= 0) if (floor[y][x - 1] == BOSS_ROOM_INDEX) continue;
                    if (x + 1 <= WIDTH) if (floor[y][x + 1] == BOSS_ROOM_INDEX) continue;
                    if (y - 1 >= 0) if (floor[y - 1][x] == BOSS_ROOM_INDEX) continue;
                    if (y + 1 <= HEIGHT) if (floor[y + 1][x] == BOSS_ROOM_INDEX) continue;

                    if (CountNeighbours(floor, x, y) >= 3)
                    {
                        floor[y][x] = SECRET_ROOM_INDEX;
                        return true;
                    }

                    if (CountNeighbours(floor, x, y) >= 2 && i > 299)
                    {
                        floor[y][x] = SECRET_ROOM_INDEX;
                        return true;
                    }

                    if (CountNeighbours(floor, x, y) >= 2 && i > 599)
                    {
                        floor[y][x] = SECRET_ROOM_INDEX;
                        return true;
                    }
                }

                return false;
            }

            (int, int) ExtractEndroom(int index)
            {
                (int, int) endroom = endrooms[index];

                endrooms.RemoveAt(index);

                return endroom;
            }
        }

        private static int CountNeighbours(int[][] floor, int x, int y)
        {
            int count = 0;

            if (x < WIDTH - 1) if (floor[y][x + 1] > 0) count++;
            if (x > 0) if (floor[y][x - 1] > 0) count++;
            if (y < HEIGHT - 1) if (floor[y + 1][x] > 0) count++;
            if (y > 0) if (floor[y - 1][x] > 0) count++;

            return count;
        }

        private static void LogFloor(int[][] floor)
        {
            for (int x = 0; x < floor.Length; x++)
            {
                for (int y = 0; y < floor[x].Length; y++)
                {
                    Console.Write(floor[x][y] + " ");
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Эта программа генерирует этажи как в компьютерной игре The binding of Isaac (не путать с The binding of Isaac: Rebirth)");
            Console.WriteLine(EMPTY_ROOM_INDEX + " - нет комнаты");
            Console.WriteLine(ROOM_INDEX + " - обычная комната");
            Console.WriteLine(BOSS_ROOM_INDEX + " - комната с боссом");
            Console.WriteLine(GOLDEN_ROOM_INDEX + " - золотая комната");
            Console.WriteLine(SHOP_INDEX + " - магазин");
            Console.WriteLine(SECRET_ROOM_INDEX + " - секретная комната");
            Console.WriteLine("Чтобы сгенерировать новую комнату, нажмите Enter");

            Console.ReadLine();

            int[][] floor;

            while (true)
            {
                floor = SetUpFloor();

                LogFloor(floor);

                Console.ReadLine();
            } 
        }
    }
}