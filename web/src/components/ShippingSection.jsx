import PropTypes from 'prop-types';
import ShippingOption from './ShippingOption';

const ShippingSection = ({ shippingCalculated, shippingOptions, selectedShipping, handleShippingSelection }) => {
    if (!shippingCalculated || shippingOptions.length === 0) {
        return null;
    }

    return (
        <div className="bg-white rounded-lg shadow-sm border p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Opções de Frete
            </h2>
            <div className="space-y-3">
                {shippingOptions.map((option) => (
                    <ShippingOption
                        key={option.serviceCode || option.serviceName}
                        option={option}
                        isSelected={selectedShipping?.serviceCode === option.serviceCode}
                        onSelect={handleShippingSelection}
                    />
                ))}
            </div>
        </div>
    );
};

ShippingSection.propTypes = {
    shippingCalculated: PropTypes.bool.isRequired,
    shippingOptions: PropTypes.arrayOf(PropTypes.shape({
        serviceName: PropTypes.string.isRequired,
        deliveryDays: PropTypes.number.isRequired,
        price: PropTypes.number.isRequired,
        observations: PropTypes.string,
        serviceCode: PropTypes.string
    })).isRequired,
    selectedShipping: PropTypes.shape({
        serviceName: PropTypes.string.isRequired,
        deliveryDays: PropTypes.number.isRequired,
        price: PropTypes.number.isRequired,
        observations: PropTypes.string,
        serviceCode: PropTypes.string
    }),
    handleShippingSelection: PropTypes.func.isRequired
};

export default ShippingSection;
