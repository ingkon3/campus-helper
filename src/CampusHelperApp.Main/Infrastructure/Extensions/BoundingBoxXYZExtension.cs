using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CampusHelperApp.Main.Infrastructure.Extensions
{
    public static class BoundingBoxXYZExtension
    {
        /// <summary>
        /// Получение величины одного из трех габаритов ящика по индексу, i - индекс габарита:
        ///     0 - длина;
        ///     1 - ширина;
        ///     2 - высота;
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static double GetDimension(this BoundingBoxXYZ boundingBox, int index) => boundingBox.Max[index] - boundingBox.Min[index];



        public enum AlignOption
        {
            Start,
            Center,
            End
        }
        public enum ModeOption
        {
            Vertical = 0,
            HorizontalDown = -1,
            HorizontalUp = 1,
        }
        public static int ToInt(this AlignOption value)
        {
            return (int)value;
        }
        public static int ToInt(this ModeOption value)
        {
            return (int)value;
        }
        public static void SetDimension(
            this BoundingBoxXYZ boundingBox, int index, double value, AlignOption alignOption = AlignOption.Center)
        {
            double minValue = -value / 2.0 * alignOption.ToInt();
            IList<double> minCoordinates = new List<double>();
            IList<double> maxCoordinates = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                minCoordinates.Add(boundingBox.Min[i]);
                maxCoordinates.Add(boundingBox.Max[i]);
            }
            minCoordinates[index] = minValue;
            maxCoordinates[index] = minValue + value;
            boundingBox.Min = new XYZ(minCoordinates[0], minCoordinates[1], minCoordinates[2]);
            boundingBox.Max = new XYZ(maxCoordinates[0], maxCoordinates[1], maxCoordinates[2]);
        }

        public static double GetLength(this BoundingBoxXYZ boundingBox) => boundingBox.GetDimension(0);
        public static void SetLength(this BoundingBoxXYZ boundingBox, double value, AlignOption alignOption = AlignOption.Center) => boundingBox.SetDimension(0, value, alignOption);

        public static double GetWidth(this BoundingBoxXYZ boundingBox) => boundingBox.GetDimension(1);
        public static void SetWidth(this BoundingBoxXYZ boundingBox, double value, AlignOption alignOption = AlignOption.Center) => boundingBox.SetDimension(1, value, alignOption);

        public static double GetHeight(this BoundingBoxXYZ boundingBox) => boundingBox.GetDimension(2);
        public static void SetHeight(this BoundingBoxXYZ boundingBox, double value, AlignOption alignOption = AlignOption.Center) => boundingBox.SetDimension(2, value, alignOption);

        public static XYZ GetOrigin(this BoundingBoxXYZ boundingBox) => boundingBox.Transform.Origin;
        public static void SetOrigin(this BoundingBoxXYZ boundingBox, XYZ origin)
        {
            Transform transform = boundingBox.Transform;
            transform.Origin = origin;
            boundingBox.Transform = transform;
        }
        public static XYZ GetCenter(this BoundingBoxXYZ boundingBox) => (boundingBox.Min + boundingBox.Max) / 2;

        public static IList<XYZ> GetBasises(this BoundingBoxXYZ boundingBox, ModeOption modeOption, XYZ orientationVector)
        {
            IList<XYZ> result = new List<XYZ>();
            if (modeOption.ToInt() != 0)
            {
                XYZ rightDirection = orientationVector;
                XYZ depthDirection = XYZ.BasisZ * modeOption.ToInt();
                XYZ upDirection = -rightDirection.CrossProduct(depthDirection);
                result.Add(rightDirection);
                result.Add(upDirection);
                result.Add(depthDirection);
            }
            else
            {
                XYZ depthDirection = orientationVector;
                XYZ upDirection = XYZ.BasisZ;
                XYZ rightDirection = -depthDirection.CrossProduct(upDirection);
                result.Add(rightDirection);
                result.Add(upDirection);
                result.Add(depthDirection);
            }
            return result;
        }

        public static void SetOrientation(this BoundingBoxXYZ boundingBoxXYZ, IList<XYZ> basises)
        {
            Transform transform = boundingBoxXYZ.Transform;
            for (int i = 0; i < 3; i++)
            {
                transform.set_Basis(i, basises[i].Normalize());
            }
            boundingBoxXYZ.Transform = transform;
        }
    }
}
