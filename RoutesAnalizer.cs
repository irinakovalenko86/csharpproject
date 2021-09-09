using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SwrElectrica.Data;
using SolidWorks.Interop.sldworks;
using SwrElectricaData.Data;
using SwrElectricaData.Data.Enum;

namespace SwrElectrica.Logic
{
    public class RoutesAnalizer
    {
        
        public List<Route> FindAllRightRoutes(Channel rootChannel, RouteWireType routeWireType = RouteWireType.Restrict)
        {
            var resultRightRoutes = new List<Route>();
            var rightRoute = new Route();

            TraverseRightRoutes(rootChannel, null, rightRoute, resultRightRoutes, routeWireType);

            return resultRightRoutes;
        }

        private Route TraverseRightRoutes(Channel startChannel, Channel rootChannel, Route rightRoute, List<Route> resultRightRoutes, RouteWireType routeWireType)
        {

            if (rootChannel != null)
            {
                if (startChannel.StartChannels.Contains(rootChannel))
                {
                    if (startChannel.FinishChannels.Count() > 0)
                    {
                        foreach (Channel rightChannel in startChannel.FinishChannels)
                        {
                            //необходима проверка на зацикливание
                            if (!rightRoute.Channels.Contains(rightChannel))
                            {
                                rightRoute.Channels.Add(rightChannel);
                                bool addRoute = false;
                                switch (routeWireType)
                                {
                                    case RouteWireType.Restrict:
                                        addRoute = false;//rightChannel.FinishChannels.Count == 0;
                                        break;
                                    case RouteWireType.EveryNode:
                                        addRoute = true;
                                        break;
                                    case RouteWireType.OnlyBreakNode:
                                        addRoute = rightChannel.FinishChannels.Count == 1 && rightChannel.StartChannels.Count <= 1;
                                        break;
                                }
                                if (addRoute)
                                {
                                    var temp = new Route();
                                    //temp.Channels.Add(startChannel);
                                    temp.Channels.AddRange(rightRoute.Channels);
                                    resultRightRoutes.Add(temp);
                                }
                                rightRoute = TraverseRightRoutes(rightChannel, startChannel, rightRoute, resultRightRoutes, routeWireType);
                                rightRoute.Channels.Remove(rightChannel);
                            }
        
                        }
                    }
                    else
                    {
                        var temp = new Route();
                        temp.Channels.AddRange(rightRoute.Channels); 
                        resultRightRoutes.Add(temp);                     
                    }
                }
                else if (startChannel.FinishChannels.Contains(rootChannel))
                {
                    if (startChannel.StartChannels.Count() > 0)
                    {
                        foreach (Channel rightChannel in startChannel.StartChannels)
                        {
                            //необходима проверка на зацикливание
                            if (!rightRoute.Channels.Contains(rightChannel))
                            {
                                rightRoute.Channels.Add(rightChannel);
                                bool addRoute = false;
                                switch (routeWireType)
                                {
                                    case RouteWireType.Restrict:
                                        addRoute = false;//rightChannel.FinishChannels.Count == 0;
                                        break;
                                    case RouteWireType.EveryNode:
                                        addRoute = true;
                                        break;
                                    case RouteWireType.OnlyBreakNode:
                                        addRoute = rightChannel.FinishChannels.Count == 1 && rightChannel.StartChannels.Count <= 1;
                                        break;
                                }
                                if (addRoute)
                                {
                                    var temp = new Route();
                                    //temp.Channels.Add(startChannel);
                                    temp.Channels.AddRange(rightRoute.Channels);
                                    resultRightRoutes.Add(temp);
                                }
                                rightRoute = TraverseRightRoutes(rightChannel, startChannel, rightRoute, resultRightRoutes, routeWireType);
                                rightRoute.Channels.Remove(rightChannel);
                            }
                        }
                    }
                    else
                    {
                        var temp = new Route();
                        temp.Channels.AddRange(rightRoute.Channels);
                        resultRightRoutes.Add(temp);
                    }
                }
                else
                {
                    var temp = new Route();
                    temp.Channels.AddRange(rightRoute.Channels);
                    resultRightRoutes.Add(temp);
           
                }
            }
            else
            {
                if (startChannel.FinishChannels.Count() > 0)
                {
                    foreach (Channel rightChannel in startChannel.FinishChannels)
                    {
                        rightRoute.Channels.Add(rightChannel);
                        bool addRoute = false;
                        switch (routeWireType)
                        {
                            case RouteWireType.Restrict:
                                addRoute = false;//startChannel.StartChannels.Count == 0 && rightChannel.FinishChannels.Count == 0;
                                break;
                            case RouteWireType.EveryNode:
                                addRoute = true;
                                break;
                            case RouteWireType.OnlyBreakNode:
                                addRoute = startChannel.StartChannels.Count <= 1 &&
                                           rightChannel.FinishChannels.Count <= 1 && rightChannel.StartChannels.Count <=1;
                                break;
                        }
                        if (addRoute)
                        {
                            var temp = new Route();
                            temp.Channels.Add(startChannel);
                            temp.Channels.AddRange(rightRoute.Channels);
                            resultRightRoutes.Add(temp);
                        }
                        rightRoute = TraverseRightRoutes(rightChannel, startChannel, rightRoute, resultRightRoutes, routeWireType);
                        rightRoute.Channels.Remove(rightChannel);
                    }
                }
                else
                {
                    var temp = new Route();
                    temp.Channels.AddRange(rightRoute.Channels);
                    resultRightRoutes.Add(temp);
                }
            }

            return rightRoute;
        }

        public List<Route> FindAllLeftRoutes(Channel rootChannel, RouteWireType routeWireType = RouteWireType.Restrict)
        {
            var resultLeftRoutes = new List<Route>();
            var leftRoute = new Route();

            TraverseLeftRoutes(rootChannel, null, leftRoute, resultLeftRoutes, routeWireType);

            return resultLeftRoutes;
        }

        private Route TraverseLeftRoutes(Channel traverseChannel, Channel rootChannel, Route leftRoute, List<Route> resultLeftRoutes, RouteWireType routeWireType)
        {
            if (rootChannel != null)
            {
                if (traverseChannel.StartChannels.Contains(rootChannel))
                {
                    if (traverseChannel.FinishChannels.Count() > 0)
                    {
                        foreach (Channel leftChannel in traverseChannel.FinishChannels)
                        {
                            //необходима проверка на зацикливание
                            if (!leftRoute.Channels.Contains(leftChannel))
                            {
                                leftRoute.Channels.Add(leftChannel);
                                bool addRoute = false;
                                switch (routeWireType)
                                {
                                    case RouteWireType.Restrict:
                                        addRoute = false;//rightChannel.FinishChannels.Count == 0;
                                        break;
                                    case RouteWireType.EveryNode:
                                        addRoute = true;
                                        break;
                                    case RouteWireType.OnlyBreakNode:
                                        addRoute = leftChannel.FinishChannels.Count == 1 && leftChannel.StartChannels.Count <= 1;
                                        break;
                                }
                                if (addRoute)
                                {
                                    var temp = new Route();
                                    //temp.Channels.Add(startChannel);
                                    temp.Channels.AddRange(leftRoute.Channels);
                                    resultLeftRoutes.Add(temp);
                                }
                                leftRoute = TraverseLeftRoutes(leftChannel, traverseChannel, leftRoute, resultLeftRoutes, routeWireType);
                                leftRoute.Channels.Remove(leftChannel);
                            }
                        }
                    }
                    else
                    {
                        var temp = new Route();
                        temp.Channels.AddRange(leftRoute.Channels);
                        resultLeftRoutes.Add(temp);
                        leftRoute.Channels.Remove(traverseChannel);
                        //leftRoute = new Route();
                    }
                }
                else if (traverseChannel.FinishChannels.Contains(rootChannel))
                {
                    if (traverseChannel.StartChannels.Count() > 0)
                    {
                        foreach (Channel leftChannel in traverseChannel.StartChannels)
                        {
                            //необходима проверка на зацикливание
                            if (!leftRoute.Channels.Contains(leftChannel))
                            {
                                leftRoute.Channels.Add(leftChannel);
                                bool addRoute = false;
                                switch (routeWireType)
                                {
                                    case RouteWireType.Restrict:
                                        addRoute = false;//rightChannel.FinishChannels.Count == 0;
                                        break;
                                    case RouteWireType.EveryNode:
                                        addRoute = true;
                                        break;
                                    case RouteWireType.OnlyBreakNode:
                                        addRoute = leftChannel.FinishChannels.Count == 1 && leftChannel.StartChannels.Count <= 1;
                                        break;
                                }
                                if (addRoute)
                                {
                                    var temp = new Route();
                                    //temp.Channels.Add(startChannel);
                                    temp.Channels.AddRange(leftRoute.Channels);
                                    resultLeftRoutes.Add(temp);
                                }
                                leftRoute = TraverseLeftRoutes(leftChannel, traverseChannel, leftRoute, resultLeftRoutes, routeWireType);
                                leftRoute.Channels.Remove(leftChannel);
                            }
                        }
                    }
                    else
                    {
                        var temp = new Route();
                        temp.Channels.AddRange(leftRoute.Channels);
                        resultLeftRoutes.Add(temp);
                        leftRoute.Channels.Remove(traverseChannel);
                        //leftRoute = new Route();
                    }
                }
                else
                {
                    var temp = new Route();
                    temp.Channels.AddRange(leftRoute.Channels);
                    resultLeftRoutes.Add(temp);
                    leftRoute.Channels.Remove(traverseChannel);
                    //leftRoute = new Route();
                }
            }
            else
            {
                if (traverseChannel.StartChannels.Count() > 0)
                {
                    foreach (Channel leftChannel in traverseChannel.StartChannels)
                    {
                        leftRoute.Channels.Add(leftChannel);
                        //resultLeftRoutes.Add(leftRoute);
                        bool addRoute = false;
                        switch (routeWireType)
                        {
                            case RouteWireType.Restrict:
                                addRoute =
                                    false; //startChannel.StartChannels.Count == 0 && rightChannel.FinishChannels.Count == 0;
                                break;
                            case RouteWireType.EveryNode:
                                addRoute = true;
                                break;
                            case RouteWireType.OnlyBreakNode:
                                addRoute = traverseChannel.StartChannels.Count <= 1 &&
                                           leftChannel.FinishChannels.Count <= 1 && leftChannel.StartChannels.Count <= 1;
                                break;
                        }
                        if (addRoute)
                        {

                            var temp = new Route();
                            temp.Channels.Add(traverseChannel);
                            temp.Channels.AddRange(leftRoute.Channels);
                            resultLeftRoutes.Add(temp);
                        }
                        leftRoute = TraverseLeftRoutes(leftChannel, traverseChannel, leftRoute, resultLeftRoutes, routeWireType);
                        //
                        leftRoute.Channels.Remove(leftChannel);
                    }
                }
            }

            return leftRoute;

        }

        /// <summary>
        /// Возвращает роут с наиболее близкими концами входа к точкам point1 и point2
        /// </summary>
        /// <param name="routes">Набор всех систем каналов, которые есть в системе</param>
        /// <param name="point1">Произвольная точка в пространстве</param>
        /// <param name="point2">Произвольная точка в пространстве</param>
        /// <param name="channel1">Ближайший к точке point1 входной канал в оптимальном роуте</param>
        /// <param name="coords1">Координата точки входа в оптимальный роут ближайшая к point1</param>
        /// <param name="sketchSegment1">Крайний скетчсегмент к которому принадлежит coords1</param>
        /// <param name="channel2">Ближайший к точке point2 входной канал в оптимальном роуте</param>
        /// <param name="coords2">Координата точки входа в оптимальный роут ближайшая к point2</param>
        /// <param name="sketchSegment2">Крайний скетчсегмент к которому принадлежит coords2</param>
        /// <returns></returns>
        public Route GetOptimalRoute(List<Route> routes, Coords point1, Coords point2, RouteWireType routeType, out Channel channel1, out Coords coords1, out SketchSegment sketchSegment1, out Channel channel2, out Coords coords2, out SketchSegment sketchSegment2, List<Channel> selectedChannels = null)
        {
            Route optimalRoute = new Route();

            channel1 = null;
            coords1 = null;
            sketchSegment1 = null;

            channel2 = null;
            coords2 = null;
            sketchSegment2 = null;

            Channel currentChannel1 = null;
            Coords currentCoords1 = null;
            SketchSegment currentSketchSegment1 = null;

            Channel currentChannel2 = null;
            Coords currentCoords2 = null;
            SketchSegment currentSketchSegment2 = null;

            double routeMeasure = -1;

            foreach (Route route in routes)
            {

                var currentMeasure = GetMeasure(route, point1, point2, routeType, out currentChannel1, out currentCoords1, out currentSketchSegment1, out currentChannel2, out currentCoords2, out currentSketchSegment2);


                if ((currentMeasure > 0 &&currentMeasure < routeMeasure) || (routeMeasure == -1 && currentMeasure > 0))
                {
                    bool allowRoute = true;
                    if (selectedChannels != null)
                    {
                        var selectedChannelInRoute = selectedChannels.Where(t => route.Channels.Contains(t));
                        allowRoute = selectedChannelInRoute.Count() == selectedChannels.Count;
                    }

                    if (allowRoute)
                    {
                        routeMeasure = currentMeasure;
                        optimalRoute = route;

                        channel1 = currentChannel1;
                        coords1 = currentCoords1;
                        sketchSegment1 = currentSketchSegment1;

                        channel2 = currentChannel2;
                        coords2 = currentCoords2;
                        sketchSegment2 = currentSketchSegment2;
                    }
                }

            }

            return optimalRoute;
        }

        /// <summary>
        /// Возвращает интегральную характеристику близости точки к системе каналов
        /// </summary>
        /// <param name="route">Система каналов</param>
        /// <param name="point1">Произвольная точка в пространстве</param>
        /// <param name="point2">Произвольная точка в пространстве</param>
        /// <param name="channel1">Ближайший к точке point1 входной канал в оптимальном роуте</param>
        /// <param name="coords1">Координата точки входа в оптимальный роут ближайшая к point1</param>
        /// <param name="sketchSegment1">Крайний скетчсегмент к которому принадлежит coords1</param>
        /// <param name="channel2">Ближайший к точке point2 входной канал в оптимальном роуте</param>
        /// <param name="coords2">Координата точки входа в оптимальный роут ближайшая к point2</param>
        /// <param name="sketchSegment2">Крайний скетчсегмент к которому принадлежит coords2</param>
        /// <returns></returns>
        private double GetMeasure(Route route, Coords point1, Coords point2, RouteWireType routeWireType, out Channel channel1, out Coords coords1, out SketchSegment sketchSegment1, out Channel channel2, out Coords coords2, out SketchSegment sketchSegment2)
        {
            var channelAnalizer = new ChannelAnalizer();
            double measure = 0;

            //получаем конечных точек роута
            var endPoints = GetRouteEndPoints(route, routeWireType);

            channel1 = null;
            coords1 = null;
            sketchSegment1 = null;
            var measure1 = GetMinimalLength(point1, endPoints, out channel1, out coords1, out sketchSegment1, null);

            channel2 = null;
            coords2 = null;
            sketchSegment2 = null;
            var measure2 = GetMinimalLength(point2, endPoints, out channel2, out coords2, out sketchSegment2, coords1);

            measure = measure1 + measure2;

            return measure;
        }

        /// <summary>
        /// Возвращает минимальное расстояние между точкой в пространстве point и конечными точками endRoutePoints какой-то системы трасс
        /// </summary>
        /// <param name="point">Точка в пространстве</param>
        /// <param name="endRoutePoints">Набор концевых точек системы трасс</param>
        /// <param name="channel">Концевой канал системы каналов до входа в который расстояние минимально от точки point</param>
        /// <param name="coords">Координаты входа в канал channel</param>
        /// <param name="sketchSegment">Концевой внешний сегмент канала channel</param>
        /// /// <param name="coordsToExcept">Координаты которые не рассматриваются в качестве претендента на coords</param>
        /// <returns></returns>
        private double GetMinimalLength(Coords point, List<Tuple<Channel, Coords, SketchSegment>> endRoutePoints, out Channel channel, out Coords coords, out SketchSegment sketchSegment, Coords coordsToExcept)
        {
            double measure = -1;
            channel = null;
            coords = null;
            sketchSegment = null;

            foreach (var tuple in endRoutePoints)
            {
                var endRoutePoint = tuple.Item2;
                var coordsManager = new CoordsManager();
                double currentMeasure = coordsManager.LengthBetweenTwoPoints(point, endRoutePoint);
                bool isPointOccupied = coordsToExcept != null && coordsManager.IsSamePoint(endRoutePoint, coordsToExcept);
                if ((currentMeasure < measure || measure == -1) && !isPointOccupied)
                {
                    measure = currentMeasure;
                    channel = tuple.Item1;
                    coords = endRoutePoint;
                    sketchSegment = tuple.Item3;
                }
            }

            return measure;
        }
        
        /// <summary>
        /// Получает конечные точки роута плюс скечсегмент, к которому эта точка относится, и трасса, к которой этот скетчсегмент принадлежит
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private List<Tuple<Channel, Coords, SketchSegment>> GetRouteEndPoints(Route route, RouteWireType routeWireType)
        {
            var channelAnalizer = new ChannelAnalizer();

            //получаем конечные трассы от Route
            var endChannels = route.GetEndChannels(routeWireType);

            //конечные точки этих трасс
            var endPoints = new List<Tuple<Channel, Coords, SketchSegment>>();

            var sketchManagerExtension = new SketchManagerExtension();

            bool onlyOneChannel = false;
            onlyOneChannel = endChannels.Count == 1;

            foreach (Channel channel in endChannels)
            {
                var endPoints1 = channelAnalizer.GetEndPoints(channel, sketchManagerExtension, routeWireType, onlyOneChannel);

                foreach (var tuple in endPoints1)
                {
                    var endPoint = tuple.Item1;
                    var sketchSegment = tuple.Item2;
                    endPoints.Add(new Tuple<Channel, Coords, SketchSegment>(channel, endPoint, sketchSegment));
                }
            }

            return endPoints;
        }

        public List<ChannelEndPoint> GetChannelEndPoints(Route route, RouteWireType routeWireType)
        {
            var channelEndPoints = new List<ChannelEndPoint>();
            var endPoints = GetRouteEndPoints(route, routeWireType);
            var coordsManager = new CoordsManager();

            foreach (var endPoint in endPoints)
            {
                var foundItem =
                    channelEndPoints.FirstOrDefault(t => coordsManager.IsSamePoint(t.ChannelEndCoords, endPoint.Item2));
                if (foundItem == null)
                {
                    var channelEndPoint = new ChannelEndPoint();
                    channelEndPoint.ChannelEndCoords = endPoint.Item2;
                    var channelEndInfo = new ChannelEndInfo();
                    channelEndInfo.Channel = endPoint.Item1;
                    channelEndInfo.ChannelSketchSegments.Add(endPoint.Item3);
                    channelEndPoint.Channels.Add(channelEndInfo);
                    channelEndPoints.Add(channelEndPoint);
                }
                else
                {
                    var foundChannel = foundItem.Channels.FirstOrDefault(t => t.Channel == endPoint.Item1);
                    if (foundChannel == null)
                    {
                        var channelInfo = new ChannelEndInfo();
                        channelInfo.Channel = endPoint.Item1;
                        channelInfo.ChannelSketchSegments.Add(endPoint.Item3);
                        foundItem.Channels.Add(channelInfo);
                    }
                }
            }

            return channelEndPoints;
        }

        //TODO: тестировать
        public void GetStartAndEndChannelsInfo(List<Coords> routeEntryCoords, List<Tuple<Channel, Coords, SketchSegment>> endPointsOfNearestRoute,
    Dictionary<Wire, List<Coords>> wireCoords, Cable cable, out Tuple<Channel, Coords, SketchSegment> endChannelInfo1, out Tuple<Channel, Coords, SketchSegment> endChannelInfo2)
        {
            endChannelInfo1 = null;
            endChannelInfo2 = null;

            var coordsManager = new CoordsManager();

            Coords optimalRouteCoord1 = null;
            Coords optimalRouteCoord2 = null;
            double minMeasure = -1;
            foreach (var wire in cable.GetWires())
            {
                var startWireCoord = wireCoords[wire][0];
                var finishWireCoord = wireCoords[wire][1];
                var startRouteCoord = coordsManager.GetNearestCoords(startWireCoord, routeEntryCoords, null);
                var finishRouteCoord = coordsManager.GetNearestCoords(finishWireCoord, routeEntryCoords, startRouteCoord);

                //получим меру для точек startRouteCoord finishRouteCoord
                var measure = GetMeasure(startRouteCoord, finishRouteCoord, wireCoords.Values.ToList());

                if (measure < minMeasure || minMeasure < 0)
                {
                    minMeasure = measure;
                    optimalRouteCoord1 = startRouteCoord;
                    optimalRouteCoord2 = finishRouteCoord;
                }
            }

            endChannelInfo1 = endPointsOfNearestRoute.Find(t => coordsManager.IsSamePoint(t.Item2, optimalRouteCoord1));
            endChannelInfo2 = endPointsOfNearestRoute.Find(t => coordsManager.IsSamePoint(t.Item2, optimalRouteCoord2));
        }

        public void GetStartAndEndChannelsInfo(List<Coords> routeEntryCoords, List<Tuple<Channel, Coords, SketchSegment>> endPointsOfNearestRoute,
Dictionary<ImportInfo, List<Coords>> wireCoords, List<ImportInfo> wireImportInfos, out Tuple<Channel, Coords, SketchSegment> endChannelInfo1, out Tuple<Channel, Coords, SketchSegment> endChannelInfo2)
        {
            endChannelInfo1 = null;
            endChannelInfo2 = null;

            var coordsManager = new CoordsManager();

            Coords optimalRouteCoord1 = null;
            Coords optimalRouteCoord2 = null;
            double minMeasure = -1;
            foreach (var wireImportInfo in wireImportInfos)
            {
                var startWireCoord = wireCoords[wireImportInfo][0];
                var finishWireCoord = wireCoords[wireImportInfo][1];
                var startRouteCoord = coordsManager.GetNearestCoords(startWireCoord, routeEntryCoords, null);
                var finishRouteCoord = coordsManager.GetNearestCoords(finishWireCoord, routeEntryCoords, startRouteCoord);

                //получим меру для точек startRouteCoord finishRouteCoord
                var measure = GetMeasure(startRouteCoord, finishRouteCoord, wireCoords.Values.ToList());

                if (measure < minMeasure || minMeasure < 0)
                {
                    minMeasure = measure;
                    optimalRouteCoord1 = startRouteCoord;
                    optimalRouteCoord2 = finishRouteCoord;
                }
            }

            endChannelInfo1 = endPointsOfNearestRoute.Find(t => coordsManager.IsSamePoint(t.Item2, optimalRouteCoord1));
            endChannelInfo2 = endPointsOfNearestRoute.Find(t => coordsManager.IsSamePoint(t.Item2, optimalRouteCoord2));
        }
        private double GetMeasure(Coords startRouteCoord, Coords finishRouteCoord, List<List<Coords>> wireInfoCoords)
        {
            double measure = 0;

            var coordsManager = new CoordsManager();
            foreach (var wireInfoCoord in wireInfoCoords)
            {
                var nearestWirePoint1 = coordsManager.GetNearestCoords(startRouteCoord, wireInfoCoord, null);
                var nearestWirePoint2 = coordsManager.GetNearestCoords(finishRouteCoord, wireInfoCoord, nearestWirePoint1);

                var l1 = coordsManager.LengthBetweenTwoPoints(startRouteCoord, nearestWirePoint1);
                var l2 = coordsManager.LengthBetweenTwoPoints(finishRouteCoord, nearestWirePoint2);

                measure = measure + l1 + l2;
            }

            return measure;
        }



        public Route GetNearestRouteForCable(List<Coords> cablePinCoords, List<Route> routes, RouteWireType routeWireType, out List<Tuple<Channel, Coords, SketchSegment>> endPointsOfNearestRoute)
        {
            Route nearestRoute = null;
            endPointsOfNearestRoute = null;

            double nearestRouteMeasure = -1;
            double routeMeasure = 0;
            foreach (var route in routes)
            {
                var endPoints = GetRouteEndPoints(route, routeWireType);

                routeMeasure = 0;
                foreach (var cablePinCoord in cablePinCoords)
                {
                    Channel channel1 = null;
                    Coords coords1 = null;
                    SketchSegment sketchSegment1 = null;
                    var minLength = GetMinimalLength(cablePinCoord, endPoints, out channel1, out coords1, out sketchSegment1, null);
                    routeMeasure = routeMeasure + minLength;
                }

                if(nearestRouteMeasure<0 || nearestRouteMeasure > routeMeasure)
                {
                    nearestRouteMeasure = routeMeasure;
                    nearestRoute = route;
                    endPointsOfNearestRoute = endPoints;
                }
            }

            return nearestRoute;
        }


        private List<Route> GetAllPathInRoute(Route route, RouteWireType routeWireType)
        {
            var result = new List<Route>();

            foreach (var routeChannel in route.Channels)
            {
                var rightRoutes = new List<Route>();
                var leftRoutes = new List<Route>();
                var unionRoutes = new List<Route>();
                if (routeChannel != null)
                {
                    if (routeWireType == RouteWireType.Restrict && routeChannel.StartChannels.Count != 0 &&
                        routeChannel.FinishChannels.Count != 0) continue;
                    bool addRoutes = true;
                    if (routeChannel.FinishChannels.Count > 0)
                    {
                        rightRoutes = FindAllRightRoutes(routeChannel, routeWireType);
                    }
                    else
                    {
                        if (routeWireType != RouteWireType.OnlyBreakNode || routeChannel.StartChannels.Count == 1)
                        {
                            var r = new Route();
                            r.Channels.Add(routeChannel);
                            rightRoutes.Add(r);
                            if (routeWireType == RouteWireType.Restrict && routeChannel.StartChannels.Count != 0)
                            {
                                addRoutes = false;
                            }
                        }
                    }


                    //пополним пути самой выделенной трассой
                    foreach (Route rightRoute in rightRoutes)
                    {
                        if (rightRoute.Channels.Count() > 0 && !rightRoute.Channels.Contains(routeChannel))
                            rightRoute.Channels.Add(routeChannel);
                    }

                    rightRoutes = RemoveDuplicateRoutes(rightRoutes);
                    if (addRoutes) unionRoutes.AddRange(rightRoutes);

                    if (routeChannel.StartChannels.Count > 0)
                    {
                        leftRoutes = FindAllLeftRoutes(routeChannel, routeWireType);
                    }

                    // формируем полные пути


                    foreach (var rightRoute in rightRoutes)
                    {
                        if (leftRoutes.Count == 0)
                        {
                            //var unionRoute = new Route();
                            //if (rightRoute.Channels.Count > 0) unionRoute.Channels.AddRange(rightRoute.Channels);
                            //if (unionRoute.Channels.Count > 0) unionRoutes.Add(unionRoute);

                        }
                        else
                        {

                            foreach (var leftRoute in leftRoutes)
                            {
                                var unionRoute = new Route();

                                if (rightRoute.Channels.Count > 0) unionRoute.Channels.AddRange(rightRoute.Channels);

                                if (leftRoute.Channels.Count > 0) unionRoute.Channels.AddRange(leftRoute.Channels);

                                if (unionRoute.Channels.Count > 0) unionRoutes.Add(unionRoute);
                            }
                        }
                    }

                    foreach (Route leftRoute in leftRoutes)
                    {
                        if (leftRoute.Channels.Count() > 0 && !leftRoute.Channels.Contains(routeChannel))
                            leftRoute.Channels.Add(routeChannel);
                    }

                    unionRoutes.AddRange(leftRoutes);

                    result.AddRange(unionRoutes);
                }
            }

            return result;
        }

        private List<Route> GetAllPathInRoute(Route route, Coords coords1, Coords coords2, RouteWireType routeWireType)
        {
            var result = new List<Route>();


            var startChannels = route.Channels
                .Where(t => t.ChannelEnd1.Coords == coords1).ToList();

            foreach (var startChannel in startChannels)
            {
                var resultRoute = new Route();
                resultRoute.Channels.Add(startChannel);

                if (startChannel.ChannelEnd2.Coords == coords2)
                {
                    result.Add(resultRoute);
                    break;
                }

                List<Route> resultRoutes;
                var isRouteFound = GetPath(startChannel.ChannelEnd2.Coords, coords2, startChannel, out resultRoutes);

                if (isRouteFound)
                {
                    foreach (var route1 in resultRoutes) route1.Channels.Add(startChannel);
                    result.AddRange(resultRoutes);
                }
            }

            var finishChannels = route.Channels.Where(t => t.ChannelEnd2.Coords == coords1).ToList();
            foreach (var finishChannel in finishChannels)
            {
                var resultRoute = new Route();
                resultRoute.Channels.Add(finishChannel);

                if (finishChannel.ChannelEnd1.Coords == coords2)
                {
                    result.Add(resultRoute);
                    break;
                }

                List<Route> resultRoutes;
                var isRouteFound = GetPath(finishChannel.ChannelEnd1.Coords, coords2, finishChannel, out resultRoutes);

                if (isRouteFound)
                {
                    foreach (var route1 in resultRoutes) route1.Channels.Add(finishChannel);
                    result.AddRange(resultRoutes);
                }
            }

            return result;
        }

        private static bool GetPath(Coords channelEndCoord, Coords coords2, Channel startChannel, out List<Route> resultRoute)
        {
            var result = false;
            resultRoute = new List<Route>();

            var coordsManager = new CoordsManager();
            var nextChannels = startChannel.ChannelEnd1.Coords == channelEndCoord ? startChannel.StartChannels : startChannel.FinishChannels;

            foreach (var nextChannel in nextChannels)
            {
                
                var channelEndCoords = coordsManager.IsSamePoint(nextChannel.ChannelEnd1.Coords, channelEndCoord)/*nextChannel.ChannelEnd1.Coords == channelEndCoord*/ ? nextChannel.ChannelEnd2.Coords : nextChannel.ChannelEnd1.Coords;
                if (channelEndCoords == coords2)
                {
                    result = true;
                    var route = new Route();
                    route.Channels.Add(nextChannel);
                    resultRoute.Add(route);
                    break;
                }

                List<Route> nextLevelRsultRoutes;
                result = GetPath(channelEndCoords, coords2, nextChannel, out nextLevelRsultRoutes);

                if (result)
                {
                    //resultRoute.Channels.Add(nextChannel);

                    //var route = new Route();
                    //route.Channels.Add(nextChannel);
                    foreach (var route1 in nextLevelRsultRoutes) route1.Channels.Add(nextChannel);
                    resultRoute.AddRange(nextLevelRsultRoutes);
                    //return result;
                }
            }

            return result || resultRoute.Count != 0;
        }

        private List<Route> RemoveDuplicateRoutes(List<Route> rightRoutes)
        {
            var result = new List<Route>();
            result.Add(rightRoutes[0]);
            for (int i = 1; i < rightRoutes.Count; i++)
            {
                var rightRoute = rightRoutes[i];
                if (!RouteAlreadyContains(result, rightRoute))
                {
                    result.Add(rightRoute);
                }
            }

            return result;
        }

        private bool RouteAlreadyContains(List<Route> routes, Route rightRoute)
        {
            var result = false;

            foreach (var route in routes)
            {
                if (route.Channels.Count == rightRoute.Channels.Count)
                {
                    var containsChannel = route.Channels.Where(t => rightRoute.Channels.Contains(t)).ToList();
                    result = containsChannel.Count == route.Channels.Count;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Метод родственнен FindShortestRoute. Ищет цепочку трасс с кратчайшим расстоянием между точками входа в систему каналов
        /// </summary>
        /// <param name="selectedChannels">Крайние каналы, между которыми нужно найти кратчайший путь</param>
        /// <returns></returns>
        public Route SearchOptimalPathInRoute(Route route, ref Coords coords1, ref Coords coords2, List<Channel> selectedChannels,
            /*List<Channel> nearestChannels,*/ RouteWireType routeWireType)
        {
            Route result = null;

            var unionRoutes = GetAllPathInRoute(route, coords1, coords2, routeWireType);


            if (selectedChannels.Count > 0)
            {
                var selectedUnionRoutes = GetRoutesContainsList(selectedChannels, unionRoutes)
                    .Where(t => t.Channels.Count != 0).ToList();

                if (selectedUnionRoutes.Count > 0)
                {
                    result = GetShortestPath(selectedUnionRoutes);
                    //CorrectCoords(result, ref coords1, ref coords2);
                }
                else
                {
                    if (selectedChannels.Count == 1)
                    {
                        var selectedChannel = selectedChannels[0];

                        if (selectedChannel.ChannelEnd1.Coords == coords1)
                        {
                            AddChannelToRoutes(unionRoutes, selectedChannel);
                            result = GetShortestPath(unionRoutes);
                            coords1 = selectedChannel.ChannelEnd2.Coords;
                        }
                        else if (selectedChannel.ChannelEnd1.Coords == coords2)
                        {
                            AddChannelToRoutes(unionRoutes, selectedChannel);
                            result = GetShortestPath(unionRoutes);
                            coords2 = selectedChannel.ChannelEnd2.Coords;
                        }
                        else if (selectedChannel.ChannelEnd2.Coords == coords1)
                        {
                            AddChannelToRoutes(unionRoutes, selectedChannel);
                            result = GetShortestPath(unionRoutes);
                            coords1 = selectedChannel.ChannelEnd1.Coords;
                        }
                        else if (selectedChannel.ChannelEnd2.Coords == coords2)
                        {
                            AddChannelToRoutes(unionRoutes, selectedChannel);
                            result = GetShortestPath(unionRoutes);
                            coords2 = selectedChannel.ChannelEnd1.Coords;
                        }
                        else
                        {
                            // выбранная трасса не на концах полученных путей
                            unionRoutes = GetAllPathInRoute(route, routeWireType);
                            selectedUnionRoutes = GetRoutesContainsList(selectedChannels, unionRoutes)
                                .Where(t => t.Channels.Count != 0).ToList();


                            result = GetShortestPath(selectedUnionRoutes);

                            CorrectCoords(result, ref coords1, ref coords2);
                        }
                    }
                    else
                    {
                        unionRoutes = GetAllPathInRoute(route, routeWireType);
                        selectedUnionRoutes = GetRoutesContainsList(selectedChannels, unionRoutes)
                            .Where(t => t.Channels.Count != 0).ToList();

                        result = GetShortestPath(selectedUnionRoutes);
                        CorrectCoords(result, ref coords1, ref coords2);
                    }

                    //if (unionRoutes.Count > 0)
                    //{
                    //    result = GetShortestPath(unionRoutes);
                    //    //if (unionRoutes.Count == 1)
                    //    //{
                    //    //    result = unionRoutes[0];
                    //    //}
                    //    //else
                    //    //{
                    //    //    double shortLength = -1;
                    //    //    foreach (var nearestUnionRoute in unionRoutes)
                    //    //    {
                    //    //        double length = 0;
                    //    //        foreach (var channel in nearestUnionRoute.Channels)
                    //    //        {
                    //    //            length = length + channel.Length;
                    //    //        }

                    //    //        if (shortLength == -1 || shortLength > length)
                    //    //        {
                    //    //            result = nearestUnionRoute;
                    //    //            shortLength = length;
                    //    //        }
                    //    //    }
                    //    //}
                    //}
                }
            }
            else
            {
                //var nearestUnionRoutes = GetRoutesContainsList(nearestChannels, unionRoutes);
                if (unionRoutes.Count > 0)
                {
                    result = GetShortestPath(unionRoutes);
                    CorrectCoords(result, ref coords1, ref coords2);
                    //if (unionRoutes.Count == 1)
                    //{
                    //    result = unionRoutes[0];
                    //}
                    //else
                    //{
                    //    double shortLength = -1;
                    //    foreach (var nearestUnionRoute in unionRoutes)
                    //    {
                    //        double length = 0;
                    //        foreach (var channel in nearestUnionRoute.Channels)
                    //        {
                    //            length = length + channel.Length;
                    //        }

                    //        if (shortLength == -1 || shortLength > length)
                    //        {
                    //            result = nearestUnionRoute;
                    //            shortLength = length;
                    //        }
                    //    }
                    //}
                }
            }

            return result;
            //Channel rootChannel = null;
            //if (selectedChannels.Count == 0)
            //{
            //    rootChannel = nearestChannels[0];
            //}
            //else
            //{
            //    rootChannel = selectedChannels[0];
            //}


            //var rightRoutes = new List<Route>();
            //var leftRoutes = new List<Route>();

            ////найдем все все возможные пути прохождения провода с finish-конца
            //if (rootChannel.FinishChannels.Count() == 0)
            //{//если на finish-конце ничего нет, то считаем что есть только один путь - сама выделенная трасса
            //    var r = new Route();
            //    r.Channels.Add(rootChannel);
            //    rightRoutes.Add(r);
            //}
            //else
            //{
            //    rightRoutes = FindAllRightRoutes(rootChannel);
            //}

            ////пополним пути самой выделенной трассой
            //foreach (Route route in rightRoutes)
            //{
            //    if (route.Channels.Count() > 0 && !route.Channels.Contains(rootChannel))
            //        route.Channels.Add(rootChannel);
            //}

            ////находим все пути с start-конца
            //if (routeWireType == RouteWireType.Restrict ||
            //    (routeWireType == RouteWireType.OnlyBreakNode && rootChannel.StartChannels.Count > 1))
            //{
            //    leftRoutes = FindAllLeftRoutes(rootChannel);
            //}

            //Route shortestRoute = null;//выступает в качестве ChannelChain
            //double lengthOfShourtestRoute = -1;

            //foreach (Route routeRight in rightRoutes)
            //{
            //    if (routeRight.Channels.Count() > 0)
            //    {
            //        if (leftRoutes.Count() == 0)
            //        {
            //            var s = new Route();
            //            leftRoutes.Add(s);
            //        }
            //        foreach (Route routeLeft in leftRoutes)
            //        {
            //            var unionRoute = new Route();//объединение какого-то правого и какого-то левого маршрута
            //            unionRoute.Channels.AddRange(routeRight.Channels);
            //            if (routeLeft.Channels.Count() != 0)
            //            {
            //                unionRoute.Channels.AddRange(routeLeft.Channels);
            //            }

            //            double length = 0;

            //            //проверяем, содержит ли маршрут все выбранные трассы
            //            bool containAll = true;
            //            for (int i = 0; i < selectedChannels.Count(); i++)
            //            {
            //                if (!unionRoute.Channels.Contains(selectedChannels[i])) { containAll = false; break; }
            //            }

            //            //если содержит, то вычисляем длину маршурта
            //            if (containAll)
            //            {
            //                //вычисляем длину объединенной трассы
            //                foreach (Channel channel in unionRoute.Channels)
            //                {
            //                    length += channel.Length;
            //                }

            //                if (lengthOfShourtestRoute > length || lengthOfShourtestRoute == -1)
            //                {
            //                    shortestRoute = unionRoute;
            //                    lengthOfShourtestRoute = length;
            //                }
            //            }
            //        }
            //    }
            //}

            //return shortestRoute;
        }

        public void CorrectCoords(Route result, ref Coords coords1, ref Coords coords2)
        {
            Coords firstEndPoint = null;
            Coords secondEndPoint = null;
            List<Coords> routeChannelCoords = new List<Coords>();

            foreach (var resultChannel in result.Channels)
            {
                firstEndPoint = resultChannel.ChannelEnd1.Coords;
                secondEndPoint = resultChannel.ChannelEnd2.Coords;

                routeChannelCoords.Add(firstEndPoint);
                routeChannelCoords.Add(secondEndPoint);

            }

            var endPoints = routeChannelCoords.FindAll(t => routeChannelCoords.FindAll(k => k == t).ToList().Count == 1);
            var coordsManager = new CoordsManager();
            var firstPointToCoords1Length = coordsManager.LengthBetweenTwoPoints(endPoints[0], coords1);
            var firstPointToCoords2Length = coordsManager.LengthBetweenTwoPoints(endPoints[0], coords2);

            var secondPointToCoords1Length = coordsManager.LengthBetweenTwoPoints(endPoints[1], coords1);
            var secondPointToCoords2Length = coordsManager.LengthBetweenTwoPoints(endPoints[1], coords2);

            if (firstPointToCoords1Length < firstPointToCoords2Length)
            {
                if (firstPointToCoords1Length < secondPointToCoords1Length)
                {
                    coords1 = endPoints[0];
                    coords2 = endPoints[1];
                }
                else
                {
                    coords1 = endPoints[1];
                    coords2 = endPoints[0];
                }
            }
            else
            {

                if (firstPointToCoords2Length < secondPointToCoords2Length)
                {
                    coords1 = endPoints[1];
                    coords2 = endPoints[0];
                }
                else
                {
                    coords1 = endPoints[0];
                    coords2 = endPoints[1];
                }
            }
        }

        private Route GetShortestPath(List<Route> selectedRoute, Coords coords1, Coords coords2)
        {
            Route result = null;
            if (selectedRoute.Count == 1)
            {
                result = selectedRoute[0];
            }
            else
            {
                double shortLength = -1;
                foreach (var nearestUnionRoute in selectedRoute)
                {

                    if (IsRouteNearCoords(nearestUnionRoute, coords1, coords2))
                    {
                        double length = 0;
                        foreach (var channel in nearestUnionRoute.Channels)
                        {
                            length = length + channel.Length;
                        }

                        if (shortLength == -1 || shortLength > length)
                        {
                            result = nearestUnionRoute;
                            shortLength = length;
                        }
                    }
                    
                }
            }
            return result;
        }

        private bool IsRouteNearCoords(Route nearestUnionRoute, Coords coords1, Coords coords2)
        {
            return true;
        }

        private static Route GetShortestPath(List<Route> selectedUnionRoutes)
        {
            Route result = null;
            if (selectedUnionRoutes.Count == 1)
            {
                result = selectedUnionRoutes[0];
            }
            else
            {
                double shortLength = -1;
                foreach (var nearestUnionRoute in selectedUnionRoutes)
                {
                    double length = 0;
                    foreach (var channel in nearestUnionRoute.Channels)
                    {
                        length = length + channel.Length;
                    }

                    if (shortLength == -1 || shortLength > length)
                    {
                        result = nearestUnionRoute;
                        shortLength = length;
                    }
                }
            }
            return result;
        }

        private static void AddChannelToRoutes(List<Route> unionRoutes, Channel selectedChannel)
        {
            foreach (var unionRoute in unionRoutes)
            {
                unionRoute.Channels.Add(selectedChannel);
            }
        }

        private static List<Route> GetRoutesContainsList(List<Channel> channels, List<Route> unionRoutes)
        {
            List<Route> nearestUnionRoutes = new List<Route>();

            foreach (var selectedUnionRoute in unionRoutes)
            {
                int count = 0;
                foreach (var nearestChannel in channels)
                {
                    if (selectedUnionRoute.Channels.Contains(nearestChannel)) count++;
                }

                if (count == channels.Count) nearestUnionRoutes.Add(selectedUnionRoute);
            }
            return nearestUnionRoutes;
        }


        public bool AreChannelsConnected(Channel channel1, Channel channel2)
        {
            List<Channel> track = new List<Channel>();

            bool resultStart = AreChannelsConnectedToTheStart(channel1, channel2, track);

            //bool resultFinish = AreChannelsConnectedToTheFinish(channel1, channel2);

            return resultStart;// || resultFinish;
        }

        private bool AreChannelsConnectedToTheStart(Channel channel1, Channel channel2, List<Channel> track)
        {
            bool result = false;

            track.Add(channel1);

            if (channel1 != channel2)
            {
                if (channel1.StartChannels.Count() > 0 || channel1.FinishChannels.Count() > 0)
                {
                    if (channel1.StartChannels.Contains(channel2)) result = true;
                    if (channel1.FinishChannels.Contains(channel2)) result = true;

                    if (result != true)
                    {

                        foreach (var channel in channel1.StartChannels)
                        {
                            if (!track.Contains(channel))
                            {
                                result = AreChannelsConnectedToTheStart(channel, channel2, track);
                                if (!result)
                                {
                                    track.Remove(channel);
                                }
                            }
                        }

                        foreach (var channel in channel1.FinishChannels)
                        {
                            if (!track.Contains(channel))
                            {
                                result = AreChannelsConnectedToTheStart(channel, channel2, track);
                                if (!result)
                                {
                                    track.Remove(channel);
                                }
                            }
                        }
                    }

                }
                else
                {
                    track.Clear();
                }
            }
            else
            {
                result = true;
            }
            

            return result;
        }

        ///поиск кратчайшего пути для кабелей
        ///
        public List<WireInformClass> FindShortestRoutesForCables(List<Channel> selectedChannels, List<Cable> selectedCables)
        {
            List<WireInformClass> routesForEachCableWithInfo = new List<WireInformClass>();
            Channel rootChannel = selectedChannels[0];
            List<Route> routesForEachCable = new List<Route>();

            //находим все маршруты в данном роутинге, содержащие выбранные трассы
            List<Route>  allRoutesInRoute = FindAllRoutesInRoute(selectedChannels);

            foreach (var cable in selectedCables)
            {
                WireInformClass inform = null;
                Route routeForCurrentCable = null;
                List<Wire> wires = new List<Wire>();
                wires = cable.GetWires();
                List<Coords> point1 = new List<Coords>();
                List<Coords> point2 = new List<Coords>();
                List<SketchSegment> segment1 = new List<SketchSegment>();
                List<SketchSegment> segment2 = new List<SketchSegment>();
                double minLength = MaxLengthOfListWires(wires, allRoutesInRoute[0], out point1, out segment1, out point2, out segment2);
                inform = new WireInformClass(point1, point2, segment1, segment2, allRoutesInRoute[0], minLength);
                routeForCurrentCable = allRoutesInRoute[0];

                foreach (var route in allRoutesInRoute)
                {
                    point1 = new List<Coords>();
                    point2 = new List<Coords>();
                    segment1 = new List<SketchSegment>();
                    segment2 = new List<SketchSegment>();

                    double currentLength = MaxLengthOfListWires(wires, route, out point1, out segment1, out point2, out segment2);
                    if (minLength > currentLength)
                    {
                        minLength = currentLength;
                        routeForCurrentCable = route;
                        inform = new WireInformClass(point1, point2, segment1, segment2, route, currentLength);
                    }
                    
                }
                routesForEachCableWithInfo.Add(inform);
                routesForEachCable.Add(routeForCurrentCable);
            }

            return routesForEachCableWithInfo;
             
        }

        private double MaxLengthOfListWires(List<Wire> wires, Route route, out List<Coords> point1, out List<SketchSegment> segment1, out List<Coords> point2, out List<SketchSegment> segment2)
        {
            point1 = new List<Coords>();
            segment1 = new List<SketchSegment>();
            point2 = new List<Coords>();
            segment2 = new List<SketchSegment>();

            double maxLength = 0;

            foreach (var wire in wires)
            {
                Coords point1Coords = null;
                Coords point2Coords = null;
                SketchSegment segment1Item = null;
                SketchSegment segment2Item = null;
                double currentLength = LengthOfWireInRoute(route, wire, out point1Coords, out segment1Item, out point2Coords, out segment2Item);
                point1.Add(point1Coords);
                point2.Add(point2Coords);
                segment1.Add(segment1Item);
                segment2.Add(segment2Item);
                if (maxLength < currentLength)
                {
                    maxLength = currentLength;
                }
            }

            return maxLength;
        }

        private double LengthOfWireInRoute(Route route, Wire wire, out Coords point1, out SketchSegment segment1, out Coords point2, out SketchSegment segment2)
        {
            point1 = null;
            point2 = null;
            segment1 = null;
            segment2 = null;

            double length = 0;
            //длина трассы
            double channelLength = 0;

            foreach (var channel in route.Channels)
            {
                channelLength += channel.Length;
            }

            //вычисляем расстояние до концов провода

            //находим крайние точки паука
            var segments = new SketchSegment[route.Channels.Count];
            var endPoints = new List<SketchPoint>();
            var indexX = new List<SketchSegment>();
            //TODO: не предусмотрено построение трассы по эскизу
            for (int k = 0; k < route.Channels.Count; k++)
            {
                segments[k] = route.Channels[k].SketchSegments[0];
            }
            var sketchManagerExtension = new SketchManagerExtension();
            sketchManagerExtension.SketchFirstPoint(segments, out endPoints, out indexX);

            if (endPoints.Count > 1)
            {
                var startWirePoint = new Coords(wire.StartPin.Coords.X,
                                                                      wire.StartPin.Coords.Y,
                                                                      wire.StartPin.Coords.Z);
                var endWirePoint = new Coords(wire.FinishPin.Coords.X,
                                                 wire.FinishPin.Coords.Y,
                                                 wire.FinishPin.Coords.Z);

                //var startPinComponent =
                //    wire.StartPin.ConnectorConfiguration.Connector.Feature.
                //        GetSpecificFeature2() as IComponent2;
                //var finishPinComponent =
                //    wire.FinishPin.ConnectorConfiguration.Connector.Feature.
                //        GetSpecificFeature2() as IComponent2;

                var startPinComponent = wire.StartPin.ConnectorConfiguration.Connector.Component;
                var finishPinComponent = wire.FinishPin.ConnectorConfiguration.Connector.Component;


                startWirePoint.ConvertFromCS(startPinComponent.GetXform() as double[]);
                endWirePoint.ConvertFromCS(finishPinComponent.GetXform() as double[]);

                var point_1 = new Coords(endPoints[0].X, endPoints[0].Y, endPoints[0].Z);
                var point_2 = new Coords(endPoints[1].X, endPoints[1].Y, endPoints[1].Z);
                var coordsManager = new CoordsManager();

                double length_1 = coordsManager.LengthBetweenTwoPoints(point_1, startWirePoint) +
                                  coordsManager.LengthBetweenTwoPoints(point_2, endWirePoint);
                double length_2 = coordsManager.LengthBetweenTwoPoints(point_2, startWirePoint) +
                                  coordsManager.LengthBetweenTwoPoints(point_1, endWirePoint);

                length = channelLength;//+ Math.Min(length_1, length_2);

                if (length_1 <= length_2)
                {
                    length += length_1;
                    point1 = point_1;
                    point2 = point_2;
                    segment1 = indexX[0];
                    segment2 = indexX[1];
                }
                else
                {
                    length += length_2;
                    point1 = point_2;
                    point2 = point_1;
                    segment1 = indexX[1];
                    segment2 = indexX[0]; 
                }


            }

            return length;
        }

        //функция возвращает все маршруты в роуте, содержащие selectedChannels. Все трассы лежат в одном роуте.
        public List<Route> FindAllRoutesInRoute(List<Channel> selectedChannels)
        {
            List<Route> unionRoutes = new List<Route>();
            
            Channel rootChannel = selectedChannels[0];

            List<Route> rightRoutes = new List<Route>();
            List<Route> leftRoutes = new List<Route>();

            //найдем все все возможные пути прохождения провода с finish-конца
            if (rootChannel.FinishChannels.Count() == 0)
            {//если на finish-конце ничего нет, то считаем что есть только один путь - сама выделенная трасса
                var r = new Route();
                r.Channels.Add(rootChannel);
                rightRoutes.Add(r);
            }
            else
            {
                rightRoutes = FindAllRightRoutes(rootChannel);
            }

            //пополним пути самой выделенной трассой
            foreach (Route route in rightRoutes)
            {
                if (route.Channels.Count() > 0 && !route.Channels.Contains(rootChannel))
                    route.Channels.Add(rootChannel);
            }

            //находим все пути с start-конца
            leftRoutes = FindAllLeftRoutes(rootChannel);

            foreach (Route routeRight in rightRoutes)
            {
                if (routeRight.Channels.Count() > 0)
                {
                    if (leftRoutes.Count() == 0)
                    {
                        Route s = new Route();
                        leftRoutes.Add(s);
                    }
                    Route unionRoute = new Route();//объединение какого-то правого и какого-то левого маршрута
                    unionRoute.Channels.AddRange(routeRight.Channels);

                    if (leftRoutes.Count > 0)
                    {
                        foreach (Route routeLeft in leftRoutes)
                        {       
                            unionRoute = new Route();//объединение какого-то правого и какого-то левого маршрута
                            unionRoute.Channels.AddRange(routeRight.Channels);

                            if (routeLeft.Channels.Count() != 0)
                            {
                                unionRoute.Channels.AddRange(routeLeft.Channels);

                                bool doesContainAll = true;
                                foreach (var channel in selectedChannels)
                                {
                                    if (!unionRoute.Channels.Contains(channel))
                                    {
                                        doesContainAll = false;
                                    }
                                }
                                if (doesContainAll)
                                {
                                    unionRoutes.Add(unionRoute);

                                    //исключаем зациклинные трассы, возможно получившиеся при объединении
                                    if (unionRoute != null)
                                    {
                                        int endsNumber = 0;
                                        foreach (var channel in unionRoute.Channels)
                                        {
                                            int linkNumber = 0;
                                            foreach (var channelItem in unionRoute.Channels)
                                            {
                                                if (channel.StartChannels.Contains(channelItem) || channel.FinishChannels.Contains(channelItem))
                                                {
                                                    linkNumber++;
                                                }
                                            }

                                            if (linkNumber == 1) endsNumber++;
                                        }

                                        if (endsNumber == 0 && unionRoute.Channels.Count>1)
                                        {
                                            unionRoutes.Remove(unionRoute);
                                        }
                                    }
                                    ////////////////////////////////////////////////////////////////////
                                }
                            }
                            else
                            {
                                bool doesContainAll = true;
                                foreach (var channel in selectedChannels)
                                {
                                    if (!unionRoute.Channels.Contains(channel))
                                    {
                                        doesContainAll = false;
                                    }
                                }
                                if (doesContainAll)
                                {
                                    unionRoutes.Add(unionRoute);

                                    //исключаем зациклинные трассы, возможно получившиеся при объединении
                                    if (unionRoute != null)
                                    {
                                        int endsNumber = 0;
                                        foreach (var channel in unionRoute.Channels)
                                        {
                                            int linkNumber = 0;
                                            foreach (var channelItem in unionRoute.Channels)
                                            {
                                                if (channel.StartChannels.Contains(channelItem) || channel.FinishChannels.Contains(channelItem))
                                                {
                                                    linkNumber++;
                                                }
                                            }

                                            if (linkNumber == 1) endsNumber++;
                                        }

                                        if (endsNumber == 0 && unionRoute.Channels.Count > 1)
                                        {
                                            unionRoutes.Remove(unionRoute);
                                        }
                                    }
                                    ////////////////////////////////////////////////////////////////////
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        bool doesContainAll = true;
                        foreach (var channel in selectedChannels)
                        {
                            if (!unionRoute.Channels.Contains(channel))
                            {
                                doesContainAll = false;
                            }
                        }
                        if (doesContainAll)
                        {
                            unionRoutes.Add(unionRoute);

                            //исключаем зациклинные трассы, возможно получившиеся при объединении
                            if (unionRoute != null)
                            {
                                int endsNumber = 0;
                                foreach (var channel in unionRoute.Channels)
                                {
                                    int linkNumber = 0;
                                    foreach (var channelItem in unionRoute.Channels)
                                    {
                                        if (channel.StartChannels.Contains(channelItem) || channel.FinishChannels.Contains(channelItem))
                                        {
                                            linkNumber++;
                                        }
                                    }

                                    if (linkNumber == 1) endsNumber++;
                                }

                                if (endsNumber == 0 && unionRoute.Channels.Count > 1)
                                {
                                    unionRoutes.Remove(unionRoute);
                                }
                            }
                            ////////////////////////////////////////////////////////////////////
                        }
                    }

                }

                
            }
            return unionRoutes;

        }

        public class WireInformClass
        {
            public List<Coords> firstEndCoords;
            public List<Coords> secondEndCoords;
            public List<SketchSegment> firstEndSketch;
            public List<SketchSegment> secondEndSketch;
            public Route route;
            public double length;

            public WireInformClass(List<Coords> firstEndCoords, List<Coords> secondEndCoords, List<SketchSegment> firstEndSketch, List<SketchSegment> secondEndSketch, Route route, double length)
            { 
                this.firstEndCoords = firstEndCoords;
                this.secondEndCoords = secondEndCoords;
                this.firstEndSketch = firstEndSketch;
                this.secondEndSketch = secondEndSketch;
                this.route = route;
                this.length = length;
            }
        }

        public void RefreshRoutes(Mounting mounting)
        {
           

            mounting.Routings.Clear();

            foreach (var channel in mounting.Channels)
            {
                if (!IsInRoute(mounting.Routings, channel))
                {
                    var route = new Route();
                    mounting.Routings.Add(route);
                    route.Channels.Add(channel);
                    channel.Route = route;

                    AddChannelsToRoute(channel,mounting.Channels, route);
                }
            }
        }

        private void AddChannelsToRoute(Channel channel, List<Channel> allChannels, Route route)
        {
            var coords1 = channel.ChannelEnd1.Coords;
            AddChannelsFromTheEnd(allChannels, channel, coords1, route);

            var coords2 = channel.ChannelEnd2.Coords;
            AddChannelsFromTheEnd(allChannels, channel, coords2, route);
        }

        private void AddChannelsFromTheEnd(List<Channel> allChannels, Channel channel, Coords coords, Route route)
        {
            var coordsManager = new CoordsManager();

            foreach (var channel1 in allChannels)
            {
                if (channel != channel1)
                {
                    if (coordsManager.IsSamePoint(channel1.ChannelEnd1.Coords, coords))
                    {
                        if (!route.Channels.Contains(channel1))
                        {
                            route.Channels.Add(channel1);
                            channel1.Route = route;

                            //рекурсия
                            AddChannelsToRoute(channel1, allChannels, route);
                        }
                    }

                    if (coordsManager.IsSamePoint(channel1.ChannelEnd2.Coords, coords))
                    {
                        if (!route.Channels.Contains(channel1))
                        {
                            route.Channels.Add(channel1);
                            channel1.Route = route;

                            //рекурсия
                            AddChannelsToRoute(channel1, allChannels, route);
                        }
                    }
                }
            }
        }

        private bool IsInRoute(List<Route> routes, Channel channel)
        {
            bool result = false;

            foreach (var route in routes)
            {
                if(route.Channels.Contains(channel))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void RefreshChannelsReferences(Route route)
        {
            foreach (var channel in route.Channels)
            {
                channel.ChannelEnd1.ChannelEnds.Clear();
                channel.ChannelEnd2.ChannelEnds.Clear();
            }

            foreach (var channel in route.Channels)
            {
                FillChannelEnd(route.Channels, channel.ChannelEnd1);

                FillChannelEnd(route.Channels, channel.ChannelEnd2);
            }
        }

        private static void FillChannelEnd(List<Channel> channels, ChannelEnd channelEnd)
        {
            var coordsManager = new CoordsManager();

            var coords = channelEnd.Coords;

            var sameEndChannels =
                    channels.FindAll(
                    t =>
                    coordsManager.IsSamePoint(t.ChannelEnd1.Coords, coords) ||
                    coordsManager.IsSamePoint(t.ChannelEnd2.Coords, coords));

 
            sameEndChannels.Remove(channelEnd.Channel);

            foreach (var sameEndChannel in sameEndChannels)
            {
                if (coordsManager.IsSamePoint(sameEndChannel.ChannelEnd1.Coords, coords))
                {
                    if (!channelEnd.ChannelEnds.Contains(sameEndChannel.ChannelEnd1))
                        channelEnd.ChannelEnds.Add(sameEndChannel.ChannelEnd1);
                }

                if (coordsManager.IsSamePoint(sameEndChannel.ChannelEnd2.Coords, coords))
                {
                    if (!channelEnd.ChannelEnds.Contains(sameEndChannel.ChannelEnd2))
                        channelEnd.ChannelEnds.Add(sameEndChannel.ChannelEnd2);
                }
            }
        }

        
        // bug 2189
        public bool IsClosedSystem(List<Channel> harnessChannels)
        {
            bool isClosedSystem = false;

            var baseChannel = harnessChannels[0];

            var currentChannel = harnessChannels[0];

            var checkedChannels = new List<Channel>();

            var channelEnd = currentChannel.ChannelEnd1;
            isClosedSystem = IsClosedSystem1(baseChannel, channelEnd, harnessChannels, ref checkedChannels);
            if (!isClosedSystem)
            {
                channelEnd = currentChannel.ChannelEnd2;
                isClosedSystem = IsClosedSystem1(baseChannel, channelEnd, harnessChannels, ref checkedChannels);
            }

            return isClosedSystem;
        }

        private bool IsClosedSystem1(Channel baseChannel, ChannelEnd channelEnd1, List<Channel> channels, ref List<Channel> checkedChannels)
        {
            bool isClosedSystem = false;

            foreach (var channelEnd in channelEnd1.ChannelEnds)
            {
                var channel = channelEnd.Channel;

                if (channels.Contains(channel) && !checkedChannels.Contains(channel))
                {

                    ChannelEnd otherChannelEnd = null;

                    if (channel.ChannelEnd1 != channelEnd) otherChannelEnd = channel.ChannelEnd1;
                    if (channel.ChannelEnd2 != channelEnd) otherChannelEnd = channel.ChannelEnd2;

                    checkedChannels.Add(channel);
                    if (otherChannelEnd.Channel == baseChannel)
                    {
                        isClosedSystem = true;
                        break;
                    }
                    else
                    {
                        isClosedSystem = IsClosedSystem1(baseChannel, otherChannelEnd, channels, ref checkedChannels);
                        if (isClosedSystem) break;
                    }
                }
            }

            return isClosedSystem;
        }


        public bool AnalizeSystemLimit(string startRefDes, string finishRefDes, double wireLength, ElectricaSettings electricaSettings)
        {
            bool result = true;
            if (electricaSettings.CommonSettings.ForbidAutoRouteProvoloka)
            {
                // убираем проволоку из списка 
            }

            if (electricaSettings.CommonSettings.ForbidAutoRouteJumper)
            {
               
                    result = startRefDes != finishRefDes;

                    if (!result) return result;
            }

            if (electricaSettings.CommonSettings.ForbidAutoRouteWire)
            {
                var length = electricaSettings.CommonSettings.ForbidAutoRouteWireLength;

                //var wireLength = Math.Round(wire.Length * 1000, 2);

                result = wireLength > length;

            }

            return result;
        }


        public Route GetNearestRouteForWire(List<Route> routes, RouteWireType routeWireType, Coords startWireCoords, Coords finishWireCoords, List<Channel> selectedChannels, out ChannelEndPoint startEndPoint, out ChannelEndPoint finishEndpoint)
        {
            Route result = null;
            startEndPoint = null;
            finishEndpoint = null;
            double minSumLength = 0;
            foreach (var route in routes)
            {
                bool checkCurrentRoute = true;
                if (selectedChannels != null && selectedChannels.Count != 0)
                {
                    checkCurrentRoute = route.Channels.Any(selectedChannels.Contains);

                }
                if (checkCurrentRoute)
                {
                    var routeEndPoints = GetChannelEndPoints(route, routeWireType);//GetRouteEndPoints(route, routeWireType);

                    double lengthToStartWireCoords = 0;
                    double lengthToFinishWireCoords = 0;
                    var firstNearestEndPoint = GetNearestEndPoint(routeEndPoints, startWireCoords, out lengthToStartWireCoords);
                    var secondNearestEndPoint = GetNearestEndPoint(routeEndPoints, finishWireCoords, out lengthToFinishWireCoords);

                    var coordsManager = new CoordsManager();
                    if (coordsManager.IsSamePoint(firstNearestEndPoint.ChannelEndCoords,
                        secondNearestEndPoint.ChannelEndCoords))
                    {
                        var foundItem = routeEndPoints.FirstOrDefault(t =>
                            coordsManager.IsSamePoint(t.ChannelEndCoords, firstNearestEndPoint.ChannelEndCoords));
                        if (foundItem != null)
                        {
                            routeEndPoints.Remove(foundItem);
                            secondNearestEndPoint = GetNearestEndPoint(routeEndPoints, finishWireCoords, out lengthToFinishWireCoords);
                        }
                    }

                    var sumLength = lengthToStartWireCoords + lengthToFinishWireCoords;
                    if (sumLength < minSumLength || minSumLength == 0)
                    {
                        minSumLength = sumLength;
                        result = route;
                        startEndPoint = firstNearestEndPoint;
                        finishEndpoint = secondNearestEndPoint;
                    }
                }

            }

            return result;
        }

        private ChannelEndPoint GetNearestEndPoint(List<ChannelEndPoint> endPoints, Coords wireCoords, out double lengthToNearestPoint)
        {
            ChannelEndPoint result = null;
            
            lengthToNearestPoint = 0;
            var coordsManager = new CoordsManager();
            int i = 0;
            foreach (var endPoint in endPoints)
            {
                double length = coordsManager.LengthBetweenTwoPoints(wireCoords, endPoint.ChannelEndCoords);
                if (length < lengthToNearestPoint || i == 0)
                {
                    i = 1;
                    lengthToNearestPoint = length;
                    result = endPoint;
                }
            }

            return result;
        }
    }
}
