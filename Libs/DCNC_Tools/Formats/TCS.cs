using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using DCNC_Tools.IO;
using DCNC_Tools.Utils.DDSReader;

namespace DCNC_Tools.Formats
{
    public class TCS
    {
        private enum TCCHUNK_ENUM
        {
            TCCHUNK_MAIN = 0x5,
            TCCHUNK_NATEDRV = 0x1000,
            TCCHUNK_NATEDRVINFO = 0x1100,
            TCCHUNK_NDNODEINFO = 0x2000,
            TCCHUNK_NDNODE = 0x2100,
            TCCHUNK_NDPATHINFO = 0x3000,
            TCCHUNK_NDPATH = 0x3100,
            TCCHUNK_NDARCINFO = 0x4000,
            TCCHUNK_NDARC = 0x4100,
            TCCHUNK_CROSSES = 0x10000,
            TCCHUNK_CROSSESINFO = 0x11000,
            TCCHUNK_CROSS = 0x12200,
            TCCHUNK_ROADS = 0x20000,
            TCCHUNK_ROADSINFO = 0x20100,
            TCCHUNK_ROAD = 0x20200,
            TCCHUNK_CROSSROADS = 0x50000,
            TCCHUNK_CROSSROADSINFO = 0x50100,
            TCCHUNK_CROSSROAD = 0x50200,
            TCCHUNK_PATHS = 0x70000,
            TCCHUNK_PATHSINFO = 0x70100,
            TCCHUNK_PATH = 0x70200,
            TCCHUNK_PATH4S = 0x70300,
            TCCHUNK_JOINTS = 0x75000,
            TCCHUNK_JOINTSINFO = 0x75100,
            TCCHUNK_JOINT = 0x75200,
            TCCHUNK_TIMEKEYDATAS = 0x90000,
            TCCHUNK_TIMEKEYDATASINFO = 0x90010,
            TCCHUNK_TIMEKEYDATA = 0x90020,
            TCCHUNK_SIGNALS = 0xA0000,
            TCCHUNK_SIGNALSINFO = 0xA0010,
            TCCHUNK_SIGNAL = 0xA0030,
            TCCHUNK_SIGNALCONTROL = 0xA0040,
            TCCHUNK_CROSSSIGNALS = 0xA5000,
            TCCHUNK_CROSSSIGNALSINFO = 0xA5010,
            TCCHUNK_CROSSSIGNAL = 0xA5030,
            TCCHUNK_SIGNAL4CLS = 0xA6000,
            TCCHUNK_SIGNAL4CLCTL = 0xA6010,
            TCCHUNK_SIGNAL4CL = 0xA6030,
        };

        public int FileVersion;

        public int FileYear;
        public int FileMonth;
        public int FileDay;

        public TCS(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReaderExt(stream))
            {
                var identifier = reader.ReadAscii();
                if (identifier != "TCS\0")
                    throw new UnknownFileFormatException();

                var header = reader.ReadAscii();
                if (header != "B\0") // Only binary TCS is supported.
                    throw new NotSupportedException();

                FileVersion = reader.ReadInt32();

                var fileDate = DateTime.ParseExact(FileVersion.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                FileDay = fileDate.Day;
                FileMonth = fileDate.Month;
                FileYear = fileDate.Year;

                //Console.WriteLine($@"Version: {FileVersion}");
                if (FileVersion != 20080327)
                    Console.WriteLine(@"Version different. Output may be wrong!");

                var identifier2 = reader.ReadAscii();
                //Console.WriteLine($@"Identifier: {identifier2}");

                Console.WriteLine(reader.ReadAscii()); // ?? (NHN-AG) (Maybe refers to NHN Corp?)
                Console.WriteLine(reader.ReadAscii()); // ?? (JC)

                // Chunk. (for whole file)
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var id = reader.ReadInt32(); // Id?
                    var len = reader.ReadInt32(); // Len?
                    //Console.WriteLine($"Id: {id} ({Enum.GetName(typeof(TCCHUNK_ENUM), id)}), Len: {len}");
                    switch ((TCCHUNK_ENUM) id)
                    {
                        /*default:
                        case TCCHUNK_ENUM.TCCHUNK_MAIN: // 0x5
                        case TCCHUNK_ENUM.TCCHUNK_NATEDRV: // 0x1000
                        case TCCHUNK_ENUM.TCCHUNK_CROSSES: // 0x10000
                        case TCCHUNK_ENUM.TCCHUNK_ROADS: // 0x20000
                        case TCCHUNK_ENUM.TCCHUNK_CROSSROADS: // 0x50000
                        case TCCHUNK_ENUM.TCCHUNK_PATHS: // 0x70000
                        case TCCHUNK_ENUM.TCCHUNK_JOINTS: // 0x75000
                        case TCCHUNK_ENUM.TCCHUNK_TIMEKEYDATAS: // 0x90000
                        case TCCHUNK_ENUM.TCCHUNK_SIGNALS: // 0xA0000
                        case TCCHUNK_ENUM.TCCHUNK_CROSSSIGNALS: // 0xA5000
                        case TCCHUNK_ENUM.TCCHUNK_SIGNAL4CLS: // 0xA6000
                            // Ignored?
                        break;*/

                        case TCCHUNK_ENUM.TCCHUNK_NATEDRVINFO: // 0x1100
                            ReadNdNodeInfo(reader); //TcTcsReader::ReadNdNodeInfo
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDNODEINFO: // 0x2000
                            ReadNdNodeInfo(reader); //TcTcsReader::ReadNdNodeInfo
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDNODE: // 0x2100
                            ReadNdNode(reader); //TcTcsReader::ReadNdNode
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDPATHINFO: // 0x3000
                            ReadNdNodeInfo(reader); //TcTcsReader::ReadNdNodeInfo
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDPATH: // 0x3100
                            ReadNdPath(reader); //TcTcsReader::ReadNdPath
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDARCINFO: // 0x4000
                            ReadNdNodeInfo(reader); //TcTcsReader::ReadNdNodeInfo
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_NDARC: // 0x4100
                            ReadNdArc(reader); //TcTcsReader::ReadNdArc
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSSESINFO: // 0x11000
                            CrossesInfoReader(reader); //TcTcsReader::CrossesInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSS: // 0x12200
                            CrossReader(reader); //TcTcsReader::CrossReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_ROADSINFO: // 0x20100
                            RoadsInfoReader(reader); //TcTcsReader::RoadsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_ROAD: // 0x20200
                            RoadReader(reader); //TcTcsReader::RoadReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSSROADSINFO: // 0x50100
                            CrossRoadsInfoReader(reader); //TcTcsReader::CrossRoadsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSSROAD: // 0x50200
                            CrossRoadReader(reader); //TcTcsReader::CrossRoadReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_PATHSINFO: // 0x70100
                            PathsInfoReader(reader); //TcTcsReader::PathsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_PATH: // 0x70200
                            PathReader(reader); //TcTcsReader::PathReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_PATH4S: // 0x70300
                            Path4SReader(reader); //TcTcsReader::Path4SReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_JOINTSINFO: // 0x75100
                            JointsInfoReader(reader); //TcTcsReader::JointsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_JOINT: // 0x75200
                            JointReader(reader); //TcTcsReader::JointReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_TIMEKEYDATASINFO: // 0x90010
                            TimeKeyDatasInfoReader(reader); //TcTcsReader::TimeKeyDatasInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_TIMEKEYDATA: // 0x90020
                            TimeKeyDataReader(reader); //TcTcsReader::TimeKeyDataReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_SIGNALSINFO: // 0xA0010
                            SignalsInfoReader(reader); //TcTcsReader::SignalsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_SIGNAL: // 0xA0030
                            SignalReader(reader); //TcTcsReader::SignalReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_SIGNALCONTROL: // 0xA0040
                            SignalControlReader(reader); //TcTcsReader::SignalControlReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSSSIGNALSINFO: // 0xA5010
                            CrossSignalsInfoReader(reader); //TcTcsReader::CrossSignalsInfoReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_CROSSSIGNAL: // 0xA5030
                            CrossSignalReader(reader); //TcTcsReader::CrossSignalReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_SIGNAL4CLCTL: // 0xA6010
                            Signals4CLCtlReader(reader); //TcTcsReader::Signals4CLCtlReader
                            break;

                        case TCCHUNK_ENUM.TCCHUNK_SIGNAL4CL: // 0xA6030
                            Signals4CLReader(reader); //TcTcsReader::Signals4CLReader
                            break;
                    }
                }
            }
        }

        private void ReadRoad(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // AID
            reader.ReadInt32(); // RID
            reader.ReadInt32(); // laneSize
            var rCount = reader.ReadInt32(); // rCount
            var lCount = reader.ReadInt32(); // lCount
            for (var i = 0; i < rCount; i++)
            {
                reader.ReadInt32(); // proad
            }
            for (var i = 0; i < lCount; i++)
            {
                reader.ReadInt32(); // proad
            }

            reader.ReadInt32(); // nodeIdx0
            reader.ReadInt32(); // arcIdx0
            reader.ReadInt32(); // nodeIdx1
            reader.ReadInt32(); // arcIdx1
        }

        private void PathsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // pathSize
        }

        private void CrossRoadReader(BinaryReaderExt reader)
        {
            ReadRoad(reader);

            reader.ReadInt32(); // RID
            reader.ReadInt32(); // nodeIdx2
            reader.ReadInt32(); // arcIdx2
            reader.ReadInt32(); // nodeIdx3
            reader.ReadInt32(); // arcIdx3
        }

        private void CrossRoadsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // crossRoadSize
        }

        private void RoadReader(BinaryReaderExt reader)
        {
            ReadRoad(reader);

            reader.ReadInt32(); // ndNodeIdx
            reader.ReadInt32(); // ndPathIdx
        }

        private void RoadsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // roadSize
        }

        private void Signals4CLReader(BinaryReaderExt reader)
        {
            var count = reader.ReadInt32(); // count
            for (var i = 0; i < count; i++)
            {
                reader.ReadInt32(); // id
                reader.ReadInt32(); // eType
                reader.ReadInt32(); // ctlID
                reader.ReadInt32(); // signalID
            }
        }

        private void Signals4CLCtlReader(BinaryReaderExt reader)
        {
            var count = reader.ReadInt32(); // count
            for (var i = 0; i < count; i++)
            {
                reader.ReadInt32(); // id
                reader.ReadInt32(); // signalID
            }
        }

        private void CrossSignalsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // crossSignalSize
        }

        private void SignalControlReader(BinaryReaderExt reader)
        {
            var max = reader.ReadInt32(); // max
            for (var i = 0; i < max; i++)
            {
                reader.ReadInt32(); // rID
            }
        }

        private void SignalsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // signalSize
        }

        private void TimeKeyDataReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // id
            var keySize = reader.ReadInt32(); // keySize
            for (var i = 0; i < keySize; i++)
            {
                reader.ReadSingle(); // time0
                reader.ReadVector3(); // pos
                reader.ReadVector3(); // right
                reader.ReadVector3(); // dir
                reader.ReadVector3(); // normal
                reader.ReadVector4(); // quat
            }
        }

        private void TimeKeyDatasInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // timekeyDataSize
        }

        private void JointReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // v15 aka AID?
            reader.ReadInt32(); // v16 aka RID?
            reader.ReadInt32(); // jointType
            var inSize = reader.ReadInt32(); // inSize
            var outSize = reader.ReadInt32(); // outSize
            for (var i = 0; i < inSize; i++)
            {
                reader.ReadInt32(); // RID
            }

            for (var i = 0; i < outSize; i++)
            {
                reader.ReadInt32(); // RID
            }

            reader.ReadInt32(); // ndNodeIdx
            reader.ReadInt32(); // arcIdx
        }

        private void JointsInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // jointSize
        }

        private void CrossReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // AID
            reader.ReadInt32(); // RID
            reader.ReadInt32(); // ndNodeIdx
            var crossRoadSize = reader.ReadInt32(); // crossRoadSize
            for (var i = 0; i < crossRoadSize; i++)
            {
                reader.ReadInt32(); // crossRoadId
            }

            var pathSize = reader.ReadInt32(); // pathSize
            for (var i = 0; i < pathSize; i++)
            {
                reader.ReadInt32(); // pathRID
            }
            reader.ReadInt32(); // crossSignalRID
        }

        private void CrossesInfoReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // count
        }

        private void ReadNdArc(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // from
            reader.ReadInt32(); // to
            reader.ReadInt32(); // pathId
        }

        private void ReadNdPath(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // from
            var pathType = 0;

            if (FileVersion >= 20061102) //if ( v3->m_iVersion >= 20061102 )
                pathType = reader.ReadInt32(); // PathType

            if (pathType == 1)
            {
                var size = reader.ReadInt32(); // size
                for (var i = 0; i < size; i++)
                {
                    LoadSinglePath(reader);
                }
            }
            else
                LoadSinglePath(reader);
        }

        private void LoadSinglePath(BinaryReaderExt reader)
        {
            /*
            if ( v3 < 20061102 )
                XiReadBTCF::Scanf((XiReadBTCF *)&this->m_Input.vfptr, "%d%d%f", &pathID, &to, &pathDist);
            else
                XiReadBTCF::Scanf((XiReadBTCF *)&this->m_Input.vfptr, "%d%d%d%f\n", &pathID, &from1, &to, &pathDist);
            */
            reader.ReadInt32(); // PathID
            reader.ReadInt32(); // From1
            reader.ReadInt32(); // to
            reader.ReadSingle(); // PathDist

            reader.ReadSingle(); // weight
            reader.ReadInt32(); // roadtype
            reader.ReadSingle(); // minSpeed
            reader.ReadSingle(); // maxSpeed


            if (FileVersion < 20061102)
                reader.ReadInt32(); // dataType
            //if ( v2->m_iVersion < 20061102 )
            // dataType


            /*
            if ( v2->m_iVersion < 20080327 )
            {
                XiReadBTCF::Scanf((XiReadBTCF *)&v2->m_Input.vfptr, "%f", &width4Road, &"|ü÷ܱG ǲҕ Ǹ");
                XiReadBTCF::Scanf((XiReadBTCF *)&v2->m_Input.vfptr, "%f", &width4Side, "?ێƍ ࠞЫҦҮ¶G Ǹ");
                hitWidth = 4.0;
                v4 = (width4Side - width4Road) * 0.5;
                lsw.fSideWay = v4;
                rsw.fSideWay = v4;
            }else
            */
            if (FileVersion < 20080327)
            {
                reader.ReadSingle(); // width4Road
                reader.ReadSingle(); // width4Side
            }
            else
            {
                reader.ReadSingle(); // hitWidth
                reader.ReadSingle(); // lsw
                reader.ReadSingle(); // lsw.fWalkW
                reader.ReadSingle(); // lsw.fWalkH

                reader.ReadSingle(); // rsw
                reader.ReadSingle(); // rsw.fWalkW
                reader.ReadSingle(); // rsw.fWalkH
            }

            reader.ReadSingle(); // laneWidth
            reader.ReadSingle(); // middleWidth
            reader.ReadInt32(); // rCount
            reader.ReadInt32(); // lCount
            reader.ReadInt32(); // bOneWay
            reader.ReadSingle(); // fStart
            reader.ReadSingle(); // fEnd
            var dataSize = reader.ReadInt32(); // dataSize
            for (var i = 0; i < dataSize; i++)
            {
                reader.ReadSingle(); // pos
                reader.ReadSingle(); // posXYZ
                reader.ReadSingle(); // posRGB
                reader.ReadSingle(); // rDist
                reader.ReadSingle(); // lDist
            }
        }

        private void CrossSignalReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // step1 aka AID
            reader.ReadInt32(); // v11 aka RID
            var roadSize = reader.ReadInt32(); // roadSize
            for (var i = 0; i < roadSize; i++)
            {
                var signalSize = reader.ReadInt32();
                for (var j = 0; j < signalSize; j++)
                {
                    reader.ReadInt32(); // RID
                }
            }
        }

        private void Path4SReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // AID
            reader.ReadInt32(); // RID
            reader.ReadSingle(); // delta
            var size = reader.ReadInt32(); // size
            for (var i = 0; i < size; i++)
            {
                reader.ReadSingle(); // f
                reader.ReadSingle(); // f
                reader.ReadSingle(); // f
            }
            //LoadPathIndex(&this->m_Input, this->m_pTCS);
        }

        private void PathReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // v11
            reader.ReadInt32(); // v12
            reader.ReadInt32(); // type
            reader.ReadSingle(); // startPosTime
            reader.ReadSingle(); // endPosTime
            reader.ReadSingle(); // maxSpeed
            reader.ReadSingle(); // stopRDist
            reader.ReadSingle(); // cruiseDist
            reader.ReadSingle(); // totalDist
            reader.ReadInt32(); // timeKeyDataId
            reader.ReadInt32(); // iDec
            reader.ReadInt32(); // iInc
            reader.ReadInt32(); // bType
            reader.ReadInt32(); // bId
            reader.ReadInt32(); // nType
            reader.ReadInt32(); // nId
            var signalSize = reader.ReadInt32(); // signalSize
            for (var i = 0; i < signalSize; i++)
            {
                reader.ReadInt32(); // id
            }
        }

        private void SignalReader(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // AID
            reader.ReadInt32(); // RID
            reader.ReadSingle(); // PosTime
            reader.ReadSingle(); // Dist
            reader.ReadInt32(); // DefaultState
            reader.ReadInt32(); // pathRID
        }

        private void ReadNdNode(BinaryReaderExt reader)
        {
            var id = reader.ReadInt32(); // Id
            var x = reader.ReadSingle(); // Pos
            var y = reader.ReadSingle(); // PosXYZ
            var z = reader.ReadSingle(); // PosRGB
            var pathSize = reader.ReadInt32(); // PathSize
            var arcSize = reader.ReadInt32(); // ArcSize

            //if ( v6 >= 20061102 )
            var eType = 0;
            if (FileVersion >= 20061102)
                eType = reader.ReadInt32(); // eType?

            Nodes.Add(new NdNode {Position = new Vector3(x, y, z), Type = eType});
        }

        private void ReadNdNodeInfo(BinaryReaderExt reader)
        {
            reader.ReadInt32(); // Arcsize?
        }

        public List<NdNode> Nodes = new List<NdNode>();
    }

    public class NdNode
    {
        public Vector3 Position;
        public int Type;
    }
}