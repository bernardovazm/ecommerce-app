import PropTypes from 'prop-types';

const ShippingOption = ({ option, isSelected, onSelect }) => {
    const handleClick = () => {
        onSelect(option);
    };

    return (
        <button
            type="button"
            className={`w-full p-3 border rounded-lg cursor-pointer transition-colors text-left ${isSelected
                ? "border-blue-500 bg-blue-50"
                : "border-gray-300 hover:border-gray-400"
                }`}
            onClick={handleClick}
            aria-pressed={isSelected}
        >
            <div className="flex justify-between items-center">
                <div>
                    <p className="font-medium text-gray-900">
                        {option.serviceName}
                    </p>
                    <p className="text-sm text-gray-600">
                        Entrega entre {option.deliveryDays} dia{option.deliveryDays > 1 ? 's' : ''} út{option.deliveryDays > 1 ? 'eis' : 'il'}
                    </p>
                    {option.observations && (
                        <p className="text-xs text-gray-500 mt-1">
                            {option.observations}
                        </p>
                    )}
                </div>
                <div className="text-right">
                    <p className="font-semibold text-gray-900">
                        R$ {option.price.toFixed(2)}
                    </p>
                    <div className={`w-4 h-4 rounded-full border-2 ml-auto mt-1 ${isSelected
                        ? "border-blue-500 bg-blue-500"
                        : "border-gray-300"
                        }`}>
                        {isSelected && (
                            <div className="w-2 h-2 bg-white rounded-full mx-auto mt-0.5"></div>
                        )}
                    </div>
                </div>
            </div>
        </button>);
};

ShippingOption.propTypes = {
    option: PropTypes.shape({
        serviceName: PropTypes.string.isRequired,
        deliveryDays: PropTypes.number.isRequired,
        price: PropTypes.number.isRequired,
        observations: PropTypes.string,
        serviceCode: PropTypes.string
    }).isRequired,
    isSelected: PropTypes.bool.isRequired,
    onSelect: PropTypes.func.isRequired
};

export default ShippingOption;
