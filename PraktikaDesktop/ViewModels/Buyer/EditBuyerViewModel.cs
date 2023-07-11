using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class EditBuyerViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        public readonly BuyerViewModel _buyerViewModel;
        public readonly SelectBuyerToSupplyDialogViewModel _selectBuyerViewModel;

        private Buyer _editableBuyer = new();
        private bool _individualSelected;
        private bool _legalEntitySelected;
        #endregion Fields

        #region Properties
        public Buyer EditableBuyer
        {
            get => _editableBuyer;
            set => this.RaiseAndSetIfChanged(ref _editableBuyer, value);
        }
        public bool IndividualSelected
        {
            get => _individualSelected;
            set => this.RaiseAndSetIfChanged(ref _individualSelected, value);
        }
        public bool LegalEntitySelected
        {
            get => _legalEntitySelected;
            set => this.RaiseAndSetIfChanged(ref _legalEntitySelected, value);
        }
        #endregion Properties

        #region Command
        public async void ClickSaveCommand()
        {
            if(_buyerViewModel != null)
            {
                if (IndividualSelected)
                {
                    if (EditableBuyer.Individual.FullName.Length == 0 || EditableBuyer.Individual.Phone.Length == 0 || EditableBuyer.Individual.SeriesPassportNumber.Length == 0 || EditableBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_buyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _buyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        EditableBuyer.LegalEntity = null;
                        _response = ApiRequest.Put("Buyer/UpdateBuyer", EditableBuyer);

                        _buyerViewModel.GoBackCommand(false);
                        _buyerViewModel.SelectedSortType = "Физ. лица";
                        _buyerViewModel.RefreshList(EditableBuyer.BuyerId);
                    }
                }
                else
                {
                    if (EditableBuyer.LegalEntity.Organization.Length == 0 || EditableBuyer.LegalEntity.Phone.Length == 0 || EditableBuyer.LegalEntity.Bank.Length == 0 || EditableBuyer.LegalEntity.CorrespondentAccount.Length == 0 ||
                        EditableBuyer.LegalEntity.CheckingAccount.Length == 0 || EditableBuyer.LegalEntity.Bic.Length == 0 || EditableBuyer.LegalEntity.Rrc.Length == 0 || EditableBuyer.LegalEntity.Tin.Length == 0 || EditableBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_buyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _buyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        EditableBuyer.Individual = null;
                        _response = ApiRequest.Put("Buyer/UpdateBuyer", EditableBuyer);

                        _buyerViewModel.GoBackCommand(false);
                        _buyerViewModel.SelectedSortType = "Юр. лица";
                        _buyerViewModel.RefreshList(EditableBuyer.BuyerId);
                    }
                }
            }
            else
            {
                if (IndividualSelected)
                {
                    if (EditableBuyer.Individual.FullName.Length == 0 || EditableBuyer.Individual.Phone.Length == 0 || EditableBuyer.Individual.SeriesPassportNumber.Length == 0 || EditableBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_selectBuyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _selectBuyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        EditableBuyer.LegalEntity = null;
                        _response = ApiRequest.Put("Buyer/UpdateBuyer", EditableBuyer);

                        _selectBuyerViewModel.GoBackCommand(false);
                        _selectBuyerViewModel.SelectedSortType = "Физ. лица";
                        _selectBuyerViewModel.RefreshList(EditableBuyer.BuyerId);
                    }
                }
                else
                {
                    if (EditableBuyer.LegalEntity.Organization.Length == 0 || EditableBuyer.LegalEntity.Phone.Length == 0 || EditableBuyer.LegalEntity.Bank.Length == 0 || EditableBuyer.LegalEntity.CorrespondentAccount.Length == 0 ||
                        EditableBuyer.LegalEntity.CheckingAccount.Length == 0 || EditableBuyer.LegalEntity.Bic.Length == 0 || EditableBuyer.LegalEntity.Rrc.Length == 0 || EditableBuyer.LegalEntity.Tin.Length == 0 || EditableBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_selectBuyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _selectBuyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        EditableBuyer.Individual = null;
                        _response = ApiRequest.Put("Buyer/UpdateBuyer", EditableBuyer);

                        _selectBuyerViewModel.GoBackCommand(false);
                        _selectBuyerViewModel.SelectedSortType = "Юр. лица";
                        _selectBuyerViewModel.RefreshList(EditableBuyer.BuyerId);
                    }
                }
            }
        }
        #endregion Command

        //Constructor
        public EditBuyerViewModel(BuyerViewModel buyerViewModel)
        {
            _buyerViewModel = buyerViewModel;

            EditableBuyer = _buyerViewModel.SelectedBuyer;

            if (_buyerViewModel.SelectedSortType == "Физ. лица")
            {
                IndividualSelected = true;
                LegalEntitySelected = false;
            }
            else
            {
                LegalEntitySelected = true;
                IndividualSelected = false;
            }
        }
        public EditBuyerViewModel(SelectBuyerToSupplyDialogViewModel selectBuyerViewModel)
        {
            _selectBuyerViewModel = selectBuyerViewModel;

            EditableBuyer = _selectBuyerViewModel.SelectedBuyer;

            if (_selectBuyerViewModel.SelectedSortType == "Физ. лица")
            {
                IndividualSelected = true;
                LegalEntitySelected = false;
            }
            else
            {
                LegalEntitySelected = true;
                IndividualSelected = false;
            }
        }
    }
}
