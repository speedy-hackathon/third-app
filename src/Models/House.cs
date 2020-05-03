namespace covidSim.Models
{
    public class House
    {
        public House(int id, Vec cornerCoordinates)
        {
            Id = id;
            Coordinates = new HouseCoordinates(cornerCoordinates);
        }

        public int Id;
        public HouseCoordinates Coordinates;
        public int ResidentCount = 0;

		public bool IsPersonInHouse(double x, double y)
		{
			return x >= Coordinates.LeftTopCorner.X && x - Coordinates.LeftTopCorner.X <= HouseCoordinates.Width
				&& y >= Coordinates.LeftTopCorner.Y && y - Coordinates.LeftTopCorner.Y <= HouseCoordinates.Height;
		}
    }
}