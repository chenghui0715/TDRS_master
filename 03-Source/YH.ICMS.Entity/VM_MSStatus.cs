using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YH.ICMS.Common;
using YH.ICMS.Common.Enumeration;

namespace YH.ICMS.Entity
{
    [Serializable]
    public class VM_MSStatus
    {
        private Direction _mDirection;
        public VM_MSStatus()
        {
           
        }
        public VM_MSStatus(Direction direction)
        {
            _mDirection = direction;
        }

        private const int BUFF_LENGTH = 8;
        #region private fields

        bool _MsStatusOne;
        bool _MsStatusTwo;
        bool _MsStatusThree;
        bool _MsStatusFour;
        bool _BakupOne;
        bool _BakupTwo;
        bool _BakupThree;
        bool _BakupFour;

        private int _nOneCount;
        private int _nTwoCount;
        private int _nThreeCount;
        private int _nFourCount;

        public bool MsStatusOne
        {
            get { return _MsStatusOne; }
            set
            {
                if (value == true)
                {
                    _nOneCount++;
                    //LogHelper.WriteInfoLog("1号磁钢计数为"+ _nOneCount);
                }
                _MsStatusOne = value;
            }
        }

        public bool MsStatusTwo
        {
            get { return _MsStatusTwo; }
            set
            {
                if (value == true)
                {
                    _nTwoCount++;
                    //LogHelper.WriteInfoLog("2号磁钢计数为" + _nTwoCount);
                }
                _MsStatusTwo = value;
            }
        }

        public bool MsStatusThree
        {
            get { return _MsStatusThree; }
            set
            {
                if (value == true)
                {
                    _nThreeCount++;
                    //LogHelper.WriteInfoLog("3号磁钢计数为" + _nThreeCount);
                }
                _MsStatusThree = value;
            }
        }

        public bool MsStatusFour
        {
            get { return _MsStatusFour; }
            set
            {
                if (value == true)
                {
                    _nFourCount++;
                    //LogHelper.WriteInfoLog("4号磁钢计数为" + _nFourCount);
                }
                _MsStatusFour = value;
            }
        }


        public bool BakupOne
        {
            get { return _BakupOne; }
            set { _BakupOne = value; }
        }
        public bool BakupTwo
        {
            get { return _BakupTwo; }
            set { _BakupTwo = value; }
        }
        public bool BakupThree
        {
            get { return _BakupThree; }
            set { _BakupThree = value; }
        }

        public bool BakupFour
        {
            get { return _BakupFour; }
            set { _BakupFour = value; }
        }

        #endregion
        public bool GetTubeIsEnter()
        {
            bool flag = false;
            if (_mDirection == Direction.入库方向)
            {
                //当有信号的时候开始判断出库还是入库的车
                if(_nFourCount >0 || _nThreeCount > 0 || _nTwoCount > 0|| _nOneCount > 0)
                {
                    if (_nFourCount > _nThreeCount || _nFourCount > _nTwoCount || _nFourCount > _nOneCount)
                    {
                        flag= false;
                        LogHelper.WriteInfoLog("该车为出库车！");
                    }
                    else if(_nFourCount < _nThreeCount || _nFourCount < _nTwoCount || _nFourCount < _nOneCount)
                    {
                        flag= true;
                        LogHelper.WriteInfoLog("该车为入库车！");
                    }
                    else if ((_nOneCount == _nTwoCount) && (_nTwoCount == _nThreeCount) && (_nThreeCount == _nFourCount) && _nFourCount > 0)
                    {
                        LogHelper.WriteInfoLog("四个磁钢信号为" + _nFourCount + "此时车已离开，清空计数！");
                        _nOneCount = 0;
                        _nTwoCount = 0;
                        _nThreeCount = 0;
                        _nFourCount = 0;
                        flag= true;
                        LogHelper.WriteInfoLog("四个磁钢信号清空计数！");
                    }
                   
                }
               
            }
            return flag;
            
        }

        public bool SetTubeLeave()
        {
            _nOneCount = 0;
            _nTwoCount = 0;
            _nThreeCount = 0;
            _nFourCount = 0;

            return true;
        }


        public bool FromBuffer(byte[] buff)
        {
            if (buff == null || buff.Length < BUFF_LENGTH)
                return false;
            MsStatusOne = bool.Parse(buff[0].ToString());
            MsStatusTwo = bool.Parse(buff[0].ToString());
            MsStatusThree = bool.Parse(buff[0].ToString());
            MsStatusFour = bool.Parse(buff[0].ToString());
            BakupOne = bool.Parse(buff[0].ToString());
            BakupTwo = bool.Parse(buff[0].ToString());
            BakupThree = bool.Parse(buff[0].ToString());
            BakupFour = bool.Parse(buff[0].ToString());

            return true;
        }
    }



}